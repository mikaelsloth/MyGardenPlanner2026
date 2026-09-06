namespace MyGardenPlanner2026.Infrastructure.Services;

using ClosedXML.Excel;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Eksporterer AuditLog-søgeresultater til CSV, JSON eller Xlsx. Genbruger
/// IAuditLogQueryService (samme filtreringslogik som visningen) — ingen duplikeret
/// filter-/sorteringskode. Henter ALLE matchende rækker uden paginering, men
/// respekterer den hårde grænse (IAuditLogExportService.HardRowLimit) defineret i PR1.
/// </summary>
public sealed class AuditLogExportService(
    IAuditLogQueryService queryService) : IAuditLogExportService
{
    private static readonly JsonSerializerOptions JsonExportOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task ExportAsync(
        AuditLogFilterDto filter,
        AuditLogExportFormat format,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(destination);

        var totalCount = await queryService.CountAsync(filter, cancellationToken);

        if (totalCount > IAuditLogExportService.HardRowLimit)
        {
            throw new InvalidOperationException(
                $"Eksporten omfatter {totalCount} rækker, hvilket overstiger den tilladte " +
                $"grænse på {IAuditLogExportService.HardRowLimit}. Indsnævr filteret og prøv igen.");
        }

        var exportFilter = filter with { PageNumber = 1, PageSize = Math.Max(totalCount, 1) };
        var result = await queryService.SearchAsync(exportFilter, cancellationToken);

        switch (format)
        {
            case AuditLogExportFormat.Csv:
                await WriteCsvAsync(result.Items, destination, cancellationToken);
                break;
            case AuditLogExportFormat.Json:
                await JsonSerializer.SerializeAsync(destination, result.Items, JsonExportOptions, cancellationToken);
                break;
            case AuditLogExportFormat.Xlsx:
                WriteXlsx(result.Items, destination);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Ukendt eksportformat.");
        }
    }

    private static async Task WriteCsvAsync(
        IReadOnlyList<AuditLogEntryDto> items, Stream destination, CancellationToken cancellationToken)
    {
        var writer = new StreamWriter(destination, Encoding.UTF8, leaveOpen: true);
        await using (writer.ConfigureAwait(false))
        {
            await writer.WriteLineAsync(
                "Id,UserId,UserEmail,IpAddress,Action,EntityName,EntityId,OldValues,NewValues,TimestampUtc");

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string[] fields =
                [
                    item.Id.ToString(CultureInfo.InvariantCulture),
                    item.UserId ?? string.Empty,
                    item.UserEmail ?? string.Empty,
                    item.IpAddress ?? string.Empty,
                    item.Action.ToString(),
                    item.EntityName,
                    item.EntityId,
                    item.OldValues ?? string.Empty,
                    item.NewValues ?? string.Empty,
                    item.TimestampUtc.ToString("O", CultureInfo.InvariantCulture)
                ];

                await writer.WriteLineAsync(string.Join(',', fields.Select(EscapeCsvField)));
            }

            await writer.FlushAsync(cancellationToken);
        }
    }

    private static string EscapeCsvField(string field) =>
        field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r')
            ? $"\"{field.Replace("\"", "\"\"")}\""
            : field;

    private static void WriteXlsx(IReadOnlyList<AuditLogEntryDto> items, Stream destination)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("AuditLog");

        string[] headers =
        [
            "Id", "UserId", "UserEmail", "IpAddress", "Action",
            "EntityName", "EntityId", "OldValues", "NewValues", "TimestampUtc"
        ];

        for (var col = 0; col < headers.Length; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
        }

        for (var row = 0; row < items.Count; row++)
        {
            var item = items[row];
            var excelRow = row + 2;

            worksheet.Cell(excelRow, 1).Value = item.Id;
            worksheet.Cell(excelRow, 2).Value = item.UserId ?? string.Empty;
            worksheet.Cell(excelRow, 3).Value = item.UserEmail ?? string.Empty;
            worksheet.Cell(excelRow, 4).Value = item.IpAddress ?? string.Empty;
            worksheet.Cell(excelRow, 5).Value = item.Action.ToString();
            worksheet.Cell(excelRow, 6).Value = item.EntityName;
            worksheet.Cell(excelRow, 7).Value = item.EntityId;
            worksheet.Cell(excelRow, 8).Value = item.OldValues ?? string.Empty;
            worksheet.Cell(excelRow, 9).Value = item.NewValues ?? string.Empty;
            worksheet.Cell(excelRow, 10).Value = item.TimestampUtc.UtcDateTime;
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(destination);
    }
}
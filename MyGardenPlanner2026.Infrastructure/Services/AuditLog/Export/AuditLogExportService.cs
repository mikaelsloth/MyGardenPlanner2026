namespace MyGardenPlanner2026.Infrastructure.Services.AuditLog.Export;

using ClosedXML.Excel;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Eksporterer AuditLog-søgeresultater til CSV, JSON eller Xlsx. Genbruger
/// IAuditLogQueryService (samme filtreringslogik som visningen). Rækker hentes i bidder
/// (keyset, BatchSize ad gangen) og skrives løbende til destinationen — hele resultatet
/// ligger aldrig i hukommelsen som liste. CSV og JSON streames. Xlsx hentes også i bidder,
/// men ClosedXML bygger hele arbejdsbogen i hukommelsen, før den gemmes. Respekterer den
/// hårde grænse (IAuditLogExportService.HardRowLimit).
/// </summary>
public sealed class AuditLogExportService(
    IAuditLogQueryService queryService) : IAuditLogExportService
{
    public const int BatchSize = 5_000;

    private const int JsonFlushThresholdBytes = 64 * 1024;

    private static readonly JsonSerializerOptions JsonExportOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonWriterOptions JsonWriterExportOptions = new() { Indented = true };

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

        var items = ReadAllAsync(filter, cancellationToken);

        switch (format)
        {
            case AuditLogExportFormat.Csv:
                await WriteCsvAsync(items, destination, cancellationToken);
                break;
            case AuditLogExportFormat.Json:
                await WriteJsonAsync(items, destination, cancellationToken);
                break;
            case AuditLogExportFormat.Xlsx:
                await WriteXlsxAsync(items, destination, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Ukendt eksportformat.");
        }
    }

    private async IAsyncEnumerable<AuditLogEntryDto> ReadAllAsync(
        AuditLogFilterDto filter, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        AuditLogCursor? cursor = null;

        while (true)
        {
            var batch = await queryService.GetBatchAsync(filter, cursor, BatchSize, cancellationToken);

            foreach (var item in batch)
            {
                yield return item;
            }

            if (batch.Count < BatchSize)
            {
                yield break;
            }

            var last = batch[^1];
            cursor = new AuditLogCursor(last.TimestampUtc, last.Id);
        }
    }

    private static async Task WriteCsvAsync(
        IAsyncEnumerable<AuditLogEntryDto> items, Stream destination, CancellationToken cancellationToken)
    {
        var writer = new StreamWriter(destination, Encoding.UTF8, leaveOpen: true);
        await using (writer.ConfigureAwait(false))
        {
            await writer.WriteLineAsync(
                "Id,UserId,UserEmail,IpAddress,Action,EntityName,EntityId,OldValues,NewValues,TimestampUtc");

            await foreach (var item in items)
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

    private static async Task WriteJsonAsync(
        IAsyncEnumerable<AuditLogEntryDto> items, Stream destination, CancellationToken cancellationToken)
    {
        var writer = new Utf8JsonWriter(destination, JsonWriterExportOptions);
        await using (writer.ConfigureAwait(false))
        {
            writer.WriteStartArray();

            await foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                JsonSerializer.Serialize(writer, item, JsonExportOptions);

                if (writer.BytesPending > JsonFlushThresholdBytes)
                {
                    await writer.FlushAsync(cancellationToken);
                }
            }

            writer.WriteEndArray();
            await writer.FlushAsync(cancellationToken);
        }
    }

    private static async Task WriteXlsxAsync(
        IAsyncEnumerable<AuditLogEntryDto> items, Stream destination, CancellationToken cancellationToken)
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

        var excelRow = 2;

        await foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            excelRow++;
        }

        worksheet.Columns().AdjustToContents();
        workbook.SaveAs(destination);
    }
}
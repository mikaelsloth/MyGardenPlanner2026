namespace MyGardenPlanner2026.Infrastructure.Services.AuditLog.Export;

using ClosedXML.Excel;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using System.Buffers;
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
    IAuditLogQueryService queryService,
    IOptionsMonitor<AuditLogExportJobOptions> optionsMonitor) : IAuditLogExportService
{
    public const int BatchSize = 5_000;

    private const int JsonFlushThresholdBytes = 64 * 1024;

    // Kolonnebredder beregnes kun ud fra overskrift og de første rækker. AdjustToContents() på alle
    // celler tog ca. 53 s og ca. 31 GB allokeringer ved 250.000 rækker (målt; nu ca. 16 s og ca. 4,8 GB).
    private const int AutoFitLastRow = 100;

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

        if (format == AuditLogExportFormat.Xlsx)
        {
            var xlsxMaxRows = Math.Max(1, optionsMonitor.CurrentValue.XlsxMaxRows);

            if (totalCount > xlsxMaxRows)
            {
                throw new InvalidOperationException(
                    IAuditLogExportService.XlsxRowLimitMessage(totalCount, xlsxMaxRows));
            }
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

    /// <summary>
    /// Skriver til en intern buffer og sender den videre med WriteAsync. Utf8JsonWriter over en Stream
    /// (og JsonSerializer.Serialize(writer, …)) flusher synkront til streamen, hvilket Kestrel ikke
    /// tillader på response body. Outputtet er identisk (indrykket JSON-array).
    /// </summary>
    private static async Task WriteJsonAsync(
        IAsyncEnumerable<AuditLogEntryDto> items, Stream destination, CancellationToken cancellationToken)
    {
        var buffer = new ArrayBufferWriter<byte>(JsonFlushThresholdBytes * 2);
        await using var writer = new Utf8JsonWriter(buffer, JsonWriterExportOptions);

        writer.WriteStartArray();

        await foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            JsonSerializer.Serialize(writer, item, JsonExportOptions);
            await writer.FlushAsync(cancellationToken);

            if (buffer.WrittenCount > JsonFlushThresholdBytes)
            {
                await destination.WriteAsync(buffer.WrittenMemory, cancellationToken);
                buffer.Clear();
            }
        }

        writer.WriteEndArray();
        await writer.FlushAsync(cancellationToken);
        await destination.WriteAsync(buffer.WrittenMemory, cancellationToken);
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

        worksheet.Columns().AdjustToContents(1, AutoFitLastRow);
        workbook.SaveAs(destination);
    }
}
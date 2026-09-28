namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Behandler ét eksportjob ad gangen. Claim sker atomisk med ExecuteUpdateAsync
/// (WHERE Status = Pending) — sikkert selv med flere app-instanser. Jobbet er ikke
/// ISoftDelete, så ExecuteUpdate omgår ingen audit-logning. DateTimeOffset-sortering
/// og -sammenligning sker i hukommelsen (SQLite-kompatibilitet).
/// Terminale statusskrivninger bruger CancellationToken.None, så en nedlukning ikke
/// efterlader et job i en tilstand, der ikke kan genskabes.
/// </summary>
public sealed partial class AuditLogExportJobProcessor(
    IAdminDbContextFactory contextFactory,
    IAuditLogQueryService queryService,
    IAuditLogExportService exportService,
    IOptionsMonitor<AuditLogExportJobOptions> optionsMonitor,
    TimeProvider timeProvider,
    ILogger<AuditLogExportJobProcessor> logger) : IAuditLogExportJobProcessor
{
    public const string GenericFailureMessage =
        "Eksporten kunne ikke gennemføres på grund af en intern fejl. Kontakt en administrator.";

    public const string StaleJobMessage =
        "Jobbet blev afbrudt (timeout eller genstart af applikationen). Start eksporten forfra.";

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private sealed record ClaimedJob(Guid Id, AuditLogExportFormat Format, string FilterJson);

    [LoggerMessage(EventId = 1122, Level = LogLevel.Information, Message = "AuditLog-eksportjob '{JobId}' gennemført: {RowCount} rækker, {FileSizeBytes} bytes.")]
    static partial void JobCompleted(ILogger logger, Guid JobId, int RowCount, int FileSizeBytes);

    [LoggerMessage(EventId = 1123, Level = LogLevel.Warning, Message = "AuditLog-eksportjob '{JobId}' fejlede: {Reason}")]
    static partial void JobFailed(ILogger logger, Guid JobId, string Reason);

    [LoggerMessage(EventId = 1124, Level = LogLevel.Information, Message = "AuditLog-eksportjob '{JobId}' sat tilbage til Pending (nedlukning).")]
    static partial void JobRequeued(ILogger logger, Guid JobId);

    [LoggerMessage(EventId = 1125, Level = LogLevel.Warning, Message = "{Count} hængende AuditLog-eksportjobs sat til Failed.")]
    static partial void StaleJobsFailed(ILogger logger, int Count);

    [LoggerMessage(EventId = 1126, Level = LogLevel.Error, Message = "Uventet fejl under behandling af AuditLog-eksportjob '{JobId}'.")]
    static partial void JobProcessingError(ILogger logger, Exception ex, Guid JobId);

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
    {
        var claimed = await ClaimNextAsync(cancellationToken);
        if (claimed is null)
        {
            return false;
        }

        try
        {
            var filter = DeserializeFilter(claimed.FilterJson);
            if (filter is null)
            {
                await FailAsync(claimed.Id, "Eksportens filter kunne ikke læses.");
                return true;
            }

            var rowCount = await queryService.CountAsync(filter, cancellationToken);
            if (rowCount > IAuditLogExportService.HardRowLimit)
            {
                await FailAsync(
                    claimed.Id,
                    $"Eksporten omfatter {rowCount} rækker, hvilket overstiger grænsen på {IAuditLogExportService.HardRowLimit}. Indsnævr filteret.");
                return true;
            }

            var stream = new MemoryStream();
            await using (stream.ConfigureAwait(false))
            {
                await exportService.ExportAsync(filter, claimed.Format, stream, cancellationToken);
                var content = stream.ToArray();

                var maxMegabytes = Math.Max(1, optionsMonitor.CurrentValue.MaxFileSizeMegabytes);
                if (content.Length > maxMegabytes * 1024L * 1024L)
                {
                    await FailAsync(claimed.Id, $"Eksportfilen overstiger den tilladte størrelse på {maxMegabytes} MB. Indsnævr filteret.");
                    return true;
                }

                await CompleteAsync(claimed, rowCount, content);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await RequeueAsync(claimed.Id);
            throw;
        }
        catch (Exception ex)
        {
            JobProcessingError(logger, ex, claimed.Id);
            await FailAsync(claimed.Id, GenericFailureMessage);
        }

        return true;
    }

    public async Task<int> FailStaleRunningJobsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var threshold = now.AddMinutes(-Math.Max(1, optionsMonitor.CurrentValue.StaleRunningMinutes));

        var running = await context.AuditLogExportJobs
            .Where(j => j.Status == AuditLogExportJobStatus.Running)
            .Select(j => new { j.Id, j.StartedAtUtc })
            .ToListAsync(cancellationToken);

        var staleIds = running
            .Where(j => j.StartedAtUtc is null || j.StartedAtUtc < threshold)
            .Select(j => j.Id)
            .ToList();

        if (staleIds.Count == 0)
        {
            return 0;
        }

        var affected = await context.AuditLogExportJobs
            .Where(j => staleIds.Contains(j.Id) && j.Status == AuditLogExportJobStatus.Running)
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Status, AuditLogExportJobStatus.Failed)
                .SetProperty(j => j.CompletedAtUtc, (DateTimeOffset?)now)
                .SetProperty(j => j.ErrorMessage, StaleJobMessage),
                cancellationToken);

        if (affected > 0)
        {
            StaleJobsFailed(logger, affected);
        }

        return affected;
    }

    private async Task<ClaimedJob?> ClaimNextAsync(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var candidates = await context.AuditLogExportJobs
            .Where(j => j.Status == AuditLogExportJobStatus.Pending)
            .Select(j => new { j.Id, j.Format, j.FilterJson, j.CreatedAtUtc })
            .ToListAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();

        foreach (var candidate in candidates.OrderBy(c => c.CreatedAtUtc))
        {
            var affected = await context.AuditLogExportJobs
                .Where(j => j.Id == candidate.Id && j.Status == AuditLogExportJobStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(j => j.Status, AuditLogExportJobStatus.Running)
                    .SetProperty(j => j.StartedAtUtc, (DateTimeOffset?)now),
                    cancellationToken);

            if (affected == 1)
            {
                return new ClaimedJob(candidate.Id, candidate.Format, candidate.FilterJson);
            }
        }

        return null;
    }

    private async Task CompleteAsync(ClaimedJob claimed, int rowCount, byte[] content)
    {
        await using var context = await contextFactory.CreateDbContextAsync(CancellationToken.None);

        var job = await context.AuditLogExportJobs
            .SingleOrDefaultAsync(j => j.Id == claimed.Id && j.Status == AuditLogExportJobStatus.Running, CancellationToken.None);

        if (job is null)
        {
            // Jobbet er slettet eller allerede markeret som Failed (fx stale-recovery) — filen kasseres.
            return;
        }

        var now = timeProvider.GetUtcNow();
        var (contentType, extension) = MapFormat(claimed.Format);

        job.Status = AuditLogExportJobStatus.Completed;
        job.CompletedAtUtc = now;
        job.ExpiresAtUtc = now.AddHours(Math.Max(1, optionsMonitor.CurrentValue.RetentionHours));
        job.RowCount = rowCount;
        job.FileName = $"audit-log-export-{now:yyyyMMdd-HHmmss}.{extension}";
        job.ContentType = contentType;
        job.FileContent = content;
        job.ErrorMessage = null;

        await context.SaveChangesAsync(CancellationToken.None);

        JobCompleted(logger, job.Id, rowCount, content.Length);
    }

    private async Task FailAsync(Guid jobId, string message)
    {
        await using var context = await contextFactory.CreateDbContextAsync(CancellationToken.None);

        var job = await context.AuditLogExportJobs
            .SingleOrDefaultAsync(j => j.Id == jobId && j.Status == AuditLogExportJobStatus.Running, CancellationToken.None);

        if (job is null)
        {
            return;
        }

        job.Status = AuditLogExportJobStatus.Failed;
        job.CompletedAtUtc = timeProvider.GetUtcNow();
        job.ErrorMessage = message;
        job.FileContent = null;

        await context.SaveChangesAsync(CancellationToken.None);

        JobFailed(logger, jobId, message);
    }

    private async Task RequeueAsync(Guid jobId)
    {
        await using var context = await contextFactory.CreateDbContextAsync(CancellationToken.None);

        var job = await context.AuditLogExportJobs
            .SingleOrDefaultAsync(j => j.Id == jobId && j.Status == AuditLogExportJobStatus.Running, CancellationToken.None);

        if (job is null)
        {
            return;
        }

        job.Status = AuditLogExportJobStatus.Pending;
        job.StartedAtUtc = null;

        await context.SaveChangesAsync(CancellationToken.None);

        JobRequeued(logger, jobId);
    }

    private static AuditLogFilterDto? DeserializeFilter(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AuditLogFilterDto>(json, SerializationOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static (string ContentType, string Extension) MapFormat(AuditLogExportFormat format) => format switch
    {
        AuditLogExportFormat.Csv => ("text/csv", "csv"),
        AuditLogExportFormat.Json => ("application/json", "json"),
        AuditLogExportFormat.Xlsx => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Ukendt eksportformat.")
    };
}
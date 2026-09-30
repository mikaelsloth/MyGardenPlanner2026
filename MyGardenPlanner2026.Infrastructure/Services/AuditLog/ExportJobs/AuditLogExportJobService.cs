namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Skriver og læser via IAdminDbContextFactory (admin-schema). Liste-forespørgsler
/// projicerer direkte til DTO, så FileContent (varbinary(max)) aldrig hentes.
/// DateTimeOffset-sortering/-sammenligning sker i hukommelsen (SQLite-kompatibilitet,
/// samme mønster som JitElevationService).
/// </summary>
public sealed partial class AuditLogExportJobService(
    IAdminDbContextFactory contextFactory,
    TimeProvider timeProvider,
    IOptionsMonitor<AuditLogExportJobOptions> optionsMonitor,
    ILogger<AuditLogExportJobService> logger) : IAuditLogExportJobService
{
    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Expression<Func<AuditLogExportJob, AuditLogExportJobDto>> ToDtoExpression =
        j => new AuditLogExportJobDto(
            j.Id, j.RequestedByUserId, j.Format, j.Status, j.CreatedAtUtc, j.StartedAtUtc,
            j.CompletedAtUtc, j.ExpiresAtUtc, j.RowCount, j.FileName, j.ErrorMessage, j.NotificationSeenAtUtc);

    private static readonly Func<AuditLogExportJob, AuditLogExportJobDto> ToDto = ToDtoExpression.Compile();

    [LoggerMessage(EventId = 1120, Level = LogLevel.Information, Message = "AuditLog-eksportjob '{JobId}' oprettet af bruger '{UserId}' (format {Format}).")]
    static partial void JobEnqueued(ILogger logger, Guid JobId, string UserId, AuditLogExportFormat Format);

    [LoggerMessage(EventId = 1121, Level = LogLevel.Information, Message = "Oprettelse af AuditLog-eksportjob afvist for bruger '{UserId}': {Reason}")]
    static partial void EnqueueRejected(ILogger logger, string UserId, string Reason);

    public async Task<AuditLogExportJobDto> EnqueueAsync(
        string userId, AuditLogFilterDto filter, AuditLogExportFormat format,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(filter);

        if (!Enum.IsDefined(format))
        {
            throw new ArgumentOutOfRangeException(nameof(format), format, "Ukendt eksportformat.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var activeCount = await context.AuditLogExportJobs.CountAsync(
            j => j.RequestedByUserId == userId
                && (j.Status == AuditLogExportJobStatus.Pending || j.Status == AuditLogExportJobStatus.Running),
            cancellationToken);

        var maxActive = Math.Max(1, optionsMonitor.CurrentValue.MaxActiveJobsPerUser);

        if (activeCount >= maxActive)
        {
            EnqueueRejected(logger, userId, "maks. antal aktive eksportjobs nået");
            throw new InvalidOperationException(
                $"Du har allerede {maxActive} igangværende eksporter. Vent til en af dem er færdig og prøv igen.");
        }

        var job = new AuditLogExportJob
        {
            RequestedByUserId = userId,
            Format = format,
            FilterJson = JsonSerializer.Serialize(filter with { PageNumber = 1 }, SerializationOptions),
            Status = AuditLogExportJobStatus.Pending,
            CreatedAtUtc = timeProvider.GetUtcNow()
        };

        await context.AuditLogExportJobs.AddAsync(job, CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        JobEnqueued(logger, job.Id, userId, format);

        return ToDto(job);
    }

    public async Task<IReadOnlyList<AuditLogExportJobDto>> GetJobsForUserAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var jobs = await context.AuditLogExportJobs
            .Where(j => j.RequestedByUserId == userId)
            .Select(ToDtoExpression)
            .ToListAsync(cancellationToken);

        return [.. jobs.OrderByDescending(j => j.CreatedAtUtc)];
    }

    public async Task<int> CountUnseenCompletedAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var expiryTimes = await context.AuditLogExportJobs
            .Where(j => j.RequestedByUserId == userId
                && j.Status == AuditLogExportJobStatus.Completed
                && j.NotificationSeenAtUtc == null)
            .Select(j => j.ExpiresAtUtc)
            .ToListAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        return expiryTimes.Count(expiresAt => expiresAt is null || expiresAt > now);
    }

    public async Task<AuditLogExportJobFileDto?> GetDownloadableFileAsync(
    Guid jobId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var job = await context.AuditLogExportJobs
            .SingleOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job is null || job.RequestedByUserId != userId || job.Status != AuditLogExportJobStatus.Completed)
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        if (job.ExpiresAtUtc is { } expiresAt && expiresAt <= now)
        {
            return null;
        }

        if (job.FileContent is null || job.FileName is null || job.ContentType is null)
        {
            return null;
        }

        if (job.NotificationSeenAtUtc is null)
        {
            job.NotificationSeenAtUtc = now;
            await context.SaveChangesAsync(cancellationToken);
        }

        return new AuditLogExportJobFileDto(job.FileName, job.ContentType, job.FileContent);
    }

    public async Task<bool> MarkNotificationSeenAsync(
        Guid jobId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var job = await context.AuditLogExportJobs
            .SingleOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job is null || job.RequestedByUserId != userId)
        {
            return false;
        }

        if (job.NotificationSeenAtUtc is null)
        {
            job.NotificationSeenAtUtc = timeProvider.GetUtcNow();
            await context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
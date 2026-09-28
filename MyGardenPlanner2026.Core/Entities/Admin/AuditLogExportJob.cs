namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Baggrundsjob til eksport af AuditLog. Implementerer BEVIDST ikke ISoftDelete: den
/// færdige fil (FileContent) ville ellers blive serialiseret ind i AuditLogs.OldValues/
/// NewValues af AuditLoggingInterceptor. Rækker og filer ryddes fysisk efter udløb
/// (samme princip som ReAuthFailureAttempt — transient mekanisme, ikke historisk sandhed).
/// </summary>
public class AuditLogExportJob
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string RequestedByUserId { get; set; } = string.Empty;

    public AuditLogExportFormat Format { get; set; }

    /// <summary>Serialiseret AuditLogFilterDto (PageNumber altid 1).</summary>
    public string FilterJson { get; set; } = string.Empty;

    public AuditLogExportJobStatus Status { get; set; } = AuditLogExportJobStatus.Pending;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>Sættes ved fuldførelse (PR 2/5). Efter dette tidspunkt er filen ikke længere tilgængelig.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public int? RowCount { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }

    public byte[]? FileContent { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>Tidspunkt hvor ejeren har set/åbnet notifikationen (styrer in-app badge).</summary>
    public DateTimeOffset? NotificationSeenAtUtc { get; set; }
}
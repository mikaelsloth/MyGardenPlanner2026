namespace MyGardenPlanner2026.Core.Contracts.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>Visningskontrakt for et eksportjob. Indeholder bevidst ikke filindholdet.</summary>
public sealed record AuditLogExportJobDto(
    Guid Id,
    string RequestedByUserId,
    AuditLogExportFormat Format,
    AuditLogExportJobStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? ExpiresAtUtc,
    int? RowCount,
    string? FileName,
    string? ErrorMessage,
    DateTimeOffset? NotificationSeenAtUtc);
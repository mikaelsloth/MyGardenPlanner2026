namespace MyGardenPlanner2026.Tests.UI;

using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Fælles builders til audit-log-DTO'er i UI-tests.
/// </summary>
public static class AuditLogTestData
{
    /// <summary>
    /// Audit-log-post. Default: Update på SubscriptionTier "abc" udført af user1@example.com uden old/new-værdier.
    /// </summary>
    public static AuditLogEntryDto Entry(
        long id = 1,
        AuditAction action = AuditAction.Update,
        string entityName = "SubscriptionTier",
        string entityId = "abc",
        string? oldValues = null,
        string? newValues = null) =>
        new(id, "user-1", "user1@example.com", "127.0.0.1", action, entityName, entityId, oldValues, newValues,
            new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    /// <summary>
    /// Baggrundseksport-job for "user-1". Default: færdigt CSV-job med 12 rækker, ikke set og uden udløb.
    /// </summary>
    public static AuditLogExportJobDto ExportJob(
        AuditLogExportJobStatus status = AuditLogExportJobStatus.Completed,
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? seenAt = null,
        int? rowCount = 12,
        string? fileName = "audit-log-export.csv",
        string? errorMessage = null) =>
        new(Guid.NewGuid(), "user-1", AuditLogExportFormat.Csv, status,
            new DateTimeOffset(2026, 9, 28, 10, 0, 0, TimeSpan.Zero), null, null,
            expiresAt, rowCount, fileName, errorMessage, seenAt);

    /// <summary>Filter uden kriterier. Sæt enkelte kriterier med <c>with { EntityName = … }</c>.</summary>
    public static AuditLogFilterDto EmptyFilter() => new(null, null, null, null, null, null, null);
}
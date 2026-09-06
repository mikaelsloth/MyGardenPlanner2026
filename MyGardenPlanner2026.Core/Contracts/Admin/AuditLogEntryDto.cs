namespace MyGardenPlanner2026.Core.Contracts.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// UI-visningskontrakt for én AuditLog-række.
/// </summary>
public sealed record AuditLogEntryDto(
    long Id,
    string? UserId,
    string? UserEmail,
    string? IpAddress,
    AuditAction Action,
    string EntityName,
    string EntityId,
    string? OldValues,
    string? NewValues,
    DateTimeOffset TimestampUtc);
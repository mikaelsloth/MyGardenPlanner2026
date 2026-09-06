namespace MyGardenPlanner2026.Core.Contracts.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Filterkriterier til søgning i admin.AuditLogs. Alle felter er valgfrie —
/// et tomt filter matcher samtlige rækker (inden for det angivne side-udsnit).
/// PageNumber er 1-indekseret. SortDescending styrer sortering på TimestampUtc.
/// </summary>
public sealed record AuditLogFilterDto(
    string? EntityName,
    string? EntityId,
    string? UserId,
    string? UserEmail,
    AuditAction? Action,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int PageNumber = 1,
    int PageSize = 25,
    bool SortDescending = true);
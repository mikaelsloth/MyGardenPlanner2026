namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Keyset-position i en AuditLog-sortering: (TimestampUtc, Id). Id er tiebreaker, så rækker
/// med identisk tidspunkt hverken springes over eller dubleres mellem bidder.
/// </summary>
public sealed record AuditLogCursor(DateTimeOffset TimestampUtc, long Id);
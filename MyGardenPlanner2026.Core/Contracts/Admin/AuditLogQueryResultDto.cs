namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Sideinddelt resultat af en AuditLog-søgning.
/// </summary>
public sealed record AuditLogQueryResultDto(
    IReadOnlyList<AuditLogEntryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Brugerens gemte visningspræferencer for AuditLog-siden.
/// LastFilter gemmes uden reelt hensyn til dets pagineringsfelter
/// (PageNumber/PageSize/SortDescending) — sidestørrelsen styres alene
/// af PageSize-feltet her ved genindlæsning.
/// </summary>
public sealed record AuditLogViewerPreferenceDto(
    int PageSize,
    AuditLogFilterDto? LastFilter);
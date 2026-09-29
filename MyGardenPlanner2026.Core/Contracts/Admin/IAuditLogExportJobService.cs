namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IAuditLogExportJobService
{
    /// <summary>
    /// Opretter et Pending-job med det angivne filter. Kaster InvalidOperationException,
    /// hvis brugeren allerede har maks. antal aktive (Pending/Running) jobs.
    /// </summary>
    Task<AuditLogExportJobDto> EnqueueAsync(
        string userId, AuditLogFilterDto filter, AuditLogExportFormat format,
        CancellationToken cancellationToken = default);

    /// <summary>Alle brugerens egne jobs, nyeste først. Indlæser aldrig filindholdet.</summary>
    Task<IReadOnlyList<AuditLogExportJobDto>> GetJobsForUserAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>Antal færdige, ikke-udløbne jobs hvor notifikationen endnu ikke er set (badge).</summary>
    Task<int> CountUnseenCompletedAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returnerer filen for download, hvis jobbet tilhører <paramref name="userId"/>, er
    /// Completed, og ikke er udløbet — ellers null. Markerer samtidig notifikationen som
    /// set, hvis den ikke allerede er det.
    /// </summary>
    Task<AuditLogExportJobFileDto?> GetDownloadableFileAsync(
        Guid jobId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Markerer notifikationen som set uden download. Returnerer true, hvis jobbet
    /// findes og tilhører brugeren (uanset om det allerede var set) — ellers false.
    /// </summary>
    Task<bool> MarkNotificationSeenAsync(
        Guid jobId, string userId, CancellationToken cancellationToken = default);
}
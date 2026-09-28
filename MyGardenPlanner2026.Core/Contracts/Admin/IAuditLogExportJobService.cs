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
}
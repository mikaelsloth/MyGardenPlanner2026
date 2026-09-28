namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IAuditLogExportJobProcessor
{
    /// <summary>
    /// Claimer det ældste Pending-job og behandler det. Returnerer false, hvis der ikke
    /// var noget job at behandle. Kaster OperationCanceledException ved nedlukning
    /// (jobbet sættes da tilbage til Pending).
    /// </summary>
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default);

    /// <summary>Sætter jobs, der har stået i Running for længe (fx efter nedbrud), til Failed. Returnerer antal.</summary>
    Task<int> FailStaleRunningJobsAsync(CancellationToken cancellationToken = default);
}
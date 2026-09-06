namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Læse-adgang til admin.AuditLogs. Rent read-only — ingen mutationer.
/// Implementeringen er provider-bevidst: fuld server-side filtrering/sortering/
/// paginering på SQL Server, in-memory fallback på SQLite (test).
/// </summary>
public interface IAuditLogQueryService
{
    Task<AuditLogQueryResultDto> SearchAsync(
        AuditLogFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>Samlet antal rækker der matcher filteret, uden hensyn til paginering.</summary>
    Task<int> CountAsync(
        AuditLogFilterDto filter, CancellationToken cancellationToken = default);
}
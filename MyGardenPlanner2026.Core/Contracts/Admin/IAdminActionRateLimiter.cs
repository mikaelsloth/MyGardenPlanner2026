namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Rate limiter til Admin CRUD-handlinger (§4.1, "AdminApiPolicy"). Partitioneret pr.
/// admin-bruger (User ID), ikke pr. IP. Bruges via AdminActionGuard i stedet for
/// Microsoft.AspNetCore.RateLimiting-middleware, da admin-editors kalder deres
/// I*AdminService direkte via DI uden et HTTP API-lag at hænge middleware på.
/// </summary>
public interface IAdminActionRateLimiter
{
    /// <summary>True hvis brugeren har ledig kvote inden for det aktuelle vindue.</summary>
    Task<bool> TryAcquireAsync(string userId, CancellationToken cancellationToken = default);
}
namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Læser/gemmer den enkelte brugers AuditLog-visningspræferencer (sidestørrelse
/// og senest anvendte filter). Ikke en §3.2-beskyttet sikkerhedspolicy — dette er
/// almindelig brugerdata, nøglet på UserId (ikke ISingletonSettings-mønsteret).
/// </summary>
public interface IAuditLogViewerPreferenceService
{
    /// <summary>Returnerer brugerens præference, eller en standardpræference (PageSize 25, intet filter) hvis ingen findes.</summary>
    Task<AuditLogViewerPreferenceDto> GetAsync(
        string userId, CancellationToken cancellationToken = default);

    Task SaveAsync(
        string userId, AuditLogViewerPreferenceDto preference, CancellationToken cancellationToken = default);
}
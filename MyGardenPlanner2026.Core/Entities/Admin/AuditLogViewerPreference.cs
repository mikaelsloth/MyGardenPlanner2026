namespace MyGardenPlanner2026.Core.Entities.Admin;

/// <summary>
/// Én bruger, én række. Gemmer sidestørrelse og senest anvendte filter for
/// AuditLog-admin-siden. Nøglet på UserId (streng, ikke fast Guid) — modsat
/// ISingletonSettings-mønsteret, da denne data er pr. bruger, ikke global.
/// Implementerer BEVIDST ikke ISoftDelete: dette er brugerpræference, ikke en
/// §3.2-beskyttet entity, og skal ikke audit-logges eller blødt slettes.
/// </summary>
public class AuditLogViewerPreference
{
    public string UserId { get; set; } = string.Empty;
    public int PageSize { get; set; } = 25;
    public string? LastFilterJson { get; set; }
}
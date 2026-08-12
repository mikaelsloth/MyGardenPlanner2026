namespace MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Abonnements-"Lag" jf. Adgang og abonnement-dokumentet.
/// IKKE identisk med arkitekturens Layer 1/2/3 (se Architecture.md).
/// Lavere numerisk værdi = højere adgangsniveau (inkluderer alle højere Lag-numre).
/// </summary>
public enum GardenAccessLevel
{
    HaveArkitekt = 1,
    BedDesigner = 2,
    Planlaegger = 3
}
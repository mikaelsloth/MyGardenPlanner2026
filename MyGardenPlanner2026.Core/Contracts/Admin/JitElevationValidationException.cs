namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Brugerrettet valideringsfejl ved JIT-anmodning (fx minutter uden for policy, manglende
/// årsag). Beskeden er på dansk og vises direkte i UI. Programmeringsfejl (fx tomt userId)
/// kaster fortsat ArgumentException, og ugyldig konfiguration kaster InvalidOperationException.
/// </summary>
public sealed class JitElevationValidationException(string message) : Exception(message);
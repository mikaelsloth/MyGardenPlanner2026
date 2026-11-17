namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Finder og gendanner haver, der er slettet ved kontosletning (R10.1b) og stadig ligger i
/// opbevaringsperioden. Matcher på hash af e-mailen (se RestoreEmailHasher).
/// OBS: Kalderen SKAL kun angive brugerens BEKRÆFTEDE e-mail. Servicen kender ikke Identity og
/// kan ikke selv afgøre, om e-mailen er bekræftet.
/// </summary>
public interface IGardenRestoreService
{
    /// <summary>Haver, der kan gendannes for e-mailen (ikke udløbne), sorteret efter navn.</summary>
    Task<IReadOnlyList<RestorableGardenDto>> GetRestorableGardensAsync(
        string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gendanner haven: un-sletter haven og flytter ejerens bevarede medlemskab og entitlement til
    /// <paramref name="userId"/> (entitlementet gendannes uændret). Sletter gendannelsesrækken.
    /// Returnerer havens Id. Kaster InvalidOperationException, hvis gendannelsen ikke findes, ikke
    /// tilhører e-mailen, er udløbet eller allerede brugt.
    /// </summary>
    Task<Guid> RestoreGardenAsync(
        Guid pendingRestoreId, string userId, string email, CancellationToken cancellationToken = default);
}
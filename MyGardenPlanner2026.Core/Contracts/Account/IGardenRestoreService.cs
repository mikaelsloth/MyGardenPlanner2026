namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Finder haver, der er slettet ved kontosletning (R10.1b) og stadig ligger i opbevaringsperioden.
/// Matcher på hash af e-mailen (se RestoreEmailHasher). Selve gendannelsen sker som del af et betalt
/// køb (IOnboardingService.ProvisionRestoredGardenAsync).
/// OBS: Kalderen SKAL kun angive brugerens BEKRÆFTEDE e-mail. Servicen kender ikke Identity og
/// kan ikke selv afgøre, om e-mailen er bekræftet.
/// </summary>
public interface IGardenRestoreService
{
    /// <summary>Haver, der kan gendannes for e-mailen (ikke udløbne), sorteret efter navn.</summary>
    Task<IReadOnlyList<RestorableGardenDto>> GetRestorableGardensAsync(
        string email, CancellationToken cancellationToken = default);
}
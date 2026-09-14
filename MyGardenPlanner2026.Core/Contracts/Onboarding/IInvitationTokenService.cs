namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// Genererer og hasher kryptografisk sikre invitations-tokens (se GardenInvitation.TokenHash).
/// Implementeringen skal bruge en CSPRNG med tilstrækkelig entropi (mindst 256 bit) og en
/// kryptografisk hash-funktion, så et databaselæk af TokenHash aldrig kan bruges til at
/// genskabe et brugbart token.
/// </summary>
public interface IInvitationTokenService
{
    /// <summary>Genererer et nyt, unikt rå token samt dets hash.</summary>
    InvitationToken GenerateToken();

    /// <summary>
    /// Hasher et modtaget rå token med samme algoritme som GenerateToken, til brug ved
    /// opslag/validering (se IOnboardingService.ValidateInvitationTokenAsync).
    /// </summary>
    string HashToken(string rawToken);
}
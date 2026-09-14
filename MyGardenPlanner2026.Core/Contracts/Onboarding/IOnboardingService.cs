namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// Orkestrerer onboarding-flows: oprettelse af gratis Sandkasse-haver, provisionering af
/// betalte haver, samt oprettelse/validering/tilbagekaldelse af haveinvitationer.
/// </summary>
public interface IOnboardingService
{
    /// <summary>
    /// Opretter en tom Sandkasse-have med begrænsede kvoter til en ny bruger, gør
    /// brugeren til ejer (IsOwner = true), og opretter et tilhørende IsTrial-entitlement.
    /// </summary>
    Task<SandboxGardenResultDto> CreateSandboxGardenAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Provisionerer en betalt have efter gennemført betaling: opretter haven, gør
    /// brugeren til ejer, og opretter det tilhørende entitlement med de valgte tilkøb.
    /// </summary>
    Task<PaidGardenProvisionResultDto> ProvisionPaidGardenAsync(
        PaidGardenProvisionRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opretter en invitation til en have. Genererer et kryptografisk sikkert rå token
    /// (kun returneret i resultatet — ALDRIG persisteret) samt dets SHA-256 hash, som
    /// gemmes på GardenInvitation.TokenHash. Trækker på inviterendes gratis
    /// invitationskvote, hvis en er tilgængelig (se GetFreeInvitationQuotaAsync).
    /// </summary>
    Task<CreateInvitationResultDto> CreateInvitationAsync(
        CreateInvitationRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validerer et råt invitations-token: hasher det og slår op mod
    /// GardenInvitation.TokenHash, og tjekker at invitationen hverken er udløbet,
    /// accepteret eller tilbagekaldt.
    /// </summary>
    Task<InvitationValidationResultDto> ValidateInvitationTokenAsync(
        string rawToken, CancellationToken cancellationToken = default);

    /// <summary>Tilbagekalder en afventende invitation (IsRevoked = true).</summary>
    Task RevokeInvitationAsync(
        Guid invitationId, string revokedByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Beregner den angivne brugers tilgængelige gratis invitationskvote for en have —
    /// én gratis invitation pr. betalt licens på samme eller lavere niveau.
    /// </summary>
    Task<FreeInvitationQuotaDto> GetFreeInvitationQuotaAsync(
        Guid gardenId, string userId, CancellationToken cancellationToken = default);
}
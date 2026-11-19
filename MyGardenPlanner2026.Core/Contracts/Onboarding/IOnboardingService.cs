namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Contracts.Account;

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

    /// <summary>
    /// Gemmer brugerens valgte konfiguration (have, lag/kategori, betalingsfrekvens,
    /// tilkøb) som en midlertidig draft, der overlever navigationen til Login/Register
    /// (static SSR, river OnboardingStateContainer ned). Udløber efter en fast levetid
    /// (se OnboardingService.CheckoutDraftLifetime).
    /// </summary>
    Task<Guid> SaveCheckoutDraftAsync(
        SaveCheckoutDraftRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Henter en gemt checkout-draft. Returnerer null hvis den ikke findes eller er udløbet.</summary>
    Task<CheckoutDraftDto?> GetCheckoutDraftAsync(
        Guid draftId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Provisionerer en betalt have ud fra en tidligere gemt draft (se
    /// SaveCheckoutDraftAsync) — kaldes efter brugeren er vendt tilbage fra
    /// Login/Register og har gennemført mock-betalingen. Sletter draften atomisk
    /// sammen med oprettelsen af have/medlemskab/entitlement.
    /// </summary>
    Task<PaidGardenProvisionResultDto> ProvisionPaidGardenFromDraftAsync(
        Guid draftId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gendanner en slettet have som del af et betalt køb (R10.1d): un-sletter haven, flytter ejerens
    /// bevarede medlemskab til brugeren med det KØBTE Layer/Category, sletter det gamle entitlement og
    /// opretter et nyt fra købet. Købet skal dække havens tidligere niveau (GardenRestoreCoverage).
    /// Kaster InvalidOperationException (uden ændringer), hvis gendannelsen ikke findes, ikke tilhører
    /// e-mailen, er udløbet, eller købet ikke dækker. <c>Email</c> skal være den BEKRÆFTEDE e-mail.
    /// </summary>
    Task<PaidGardenProvisionResultDto> ProvisionRestoredGardenAsync(
        PaidGardenRestoreRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Som ProvisionRestoredGardenAsync, men køb (niveau, kategori, frekvens, tilkøb) hentes fra en
    /// gemt checkout-draft, som slettes atomisk. Ved afvisning bevares draften.
    /// </summary>
    Task<PaidGardenProvisionResultDto> ProvisionRestoredGardenFromDraftAsync(
        Guid draftId, string userId, string email, Guid restorePendingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Accepterer en invitation: opretter GardenMembership til brugeren på det angivne
    /// niveau (GrantedLayer/GrantedCategory, valideret mod invitationens
    /// TargetLayer/MaxAllowedLayer-loft), markerer invitationen som accepteret, og —
    /// hvis brugeren selv har betalt for en opgradering (UpgradeBillingCycle angivet) —
    /// opretter ét samlet UserEntitlement direkte på det opgraderede niveau. Uden
    /// selvbetalt opgradering oprettes intet entitlement.
    /// </summary>
    Task<AcceptInvitationResultDto> AcceptInvitationAsync(
        AcceptInvitationRequestDto request, CancellationToken cancellationToken = default);
}
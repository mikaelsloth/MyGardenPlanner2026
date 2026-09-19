namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// Læse-adgang til Garden/GardenMembership/GardenInvitation. Bruges af
/// GardenAccessManagementPage og GardenMemberAuthorizationHandler. Mutationer sker
/// udelukkende via IOnboardingService.
/// </summary>
public interface IGardenAccessQueryService
{
    Task<GardenSummaryDto?> GetGardenSummaryAsync(Guid gardenId, CancellationToken cancellationToken = default);

    Task<GardenMembershipDto?> GetMembershipAsync(Guid gardenId, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GardenMembershipDto>> GetMembersAsync(Guid gardenId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GardenInvitationDto>> GetInvitationsAsync(Guid gardenId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tæller brugerens EGNE (IsOwner) haver, opdelt på aktive/arkiverede — bruges til at
    /// vise korrekt volumenrabat-trin i onboarding-checkout for allerede
    /// autentificerede brugere, der opretter endnu en have.
    /// </summary>
    Task<OwnedGardenCountsDto> GetOwnedGardenCountsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// True hvis brugeren har mindst ét GardenMembership (som ejer eller inviteret medlem)
    /// OG mindst ét ikke-udløbet UserEntitlement (trial eller betalt). Bruges af
    /// OnboardingGate til at afgøre, om brugeren skal sendes til onboarding-welcome.
    /// Begge tjekkes, da et medlemskab principielt kan eksistere uden et gyldigt
    /// entitlement (fx et udløbet trial) — i praksis opretter alle nuværende flows
    /// (Sandkasse, betalt have, accepteret invitation) altid begge dele samlet.
    /// </summary>
    Task<bool> HasAnyGardenAccessAsync(string userId, CancellationToken cancellationToken = default);
}
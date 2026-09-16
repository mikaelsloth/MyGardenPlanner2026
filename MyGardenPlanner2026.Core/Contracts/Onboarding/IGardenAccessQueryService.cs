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
}
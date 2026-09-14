namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Entities.Common;

public sealed record CreateInvitationRequestDto(
    Guid GardenId,
    string InvitedByUserId,
    string Email,
    GardenAccessLevel TargetLayer,
    AccessCategory TargetCategory,
    GardenAccessLevel MaxAllowedLayer,
    AccessCategory MaxAllowedCategory,
    bool AllowSelfUpgrade,
    TimeSpan ValidFor);
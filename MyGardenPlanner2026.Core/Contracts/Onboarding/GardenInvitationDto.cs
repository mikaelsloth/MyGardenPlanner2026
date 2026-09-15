namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>UI-visningskontrakt for en invitation. Indeholder bevidst ikke TokenHash.</summary>
public sealed record GardenInvitationDto(
    Guid Id,
    Guid GardenId,
    string InvitedByUserId,
    string Email,
    GardenAccessLevel TargetLayer,
    AccessCategory TargetCategory,
    bool IsFreeSlot,
    bool AllowSelfUpgrade,
    DateTimeOffset ExpiresUtc,
    bool IsAccepted,
    bool IsRevoked,
    DateTimeOffset CreatedAtUtc);
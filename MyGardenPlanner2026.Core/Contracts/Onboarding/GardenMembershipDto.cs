namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Entities.Common;

public sealed record GardenMembershipDto(
    Guid Id,
    Guid GardenId,
    string UserId,
    bool IsOwner,
    GardenAccessLevel Layer,
    AccessCategory Category,
    DateTimeOffset JoinedAtUtc);
namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

public sealed record PaidGardenProvisionResultDto(
    Guid GardenId,
    Guid MembershipId,
    Guid EntitlementId);
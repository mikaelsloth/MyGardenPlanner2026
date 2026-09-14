namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

public sealed record SandboxGardenResultDto(
    Guid GardenId,
    Guid MembershipId,
    Guid EntitlementId);
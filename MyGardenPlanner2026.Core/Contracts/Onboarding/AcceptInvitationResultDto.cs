namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

public sealed record AcceptInvitationResultDto(
    Guid GardenId,
    Guid MembershipId,
    Guid? EntitlementId);
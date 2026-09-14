namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

public sealed record InvitationValidationResultDto(
    bool IsValid,
    GardenInvitationDto? Invitation,
    string? FailureReason);
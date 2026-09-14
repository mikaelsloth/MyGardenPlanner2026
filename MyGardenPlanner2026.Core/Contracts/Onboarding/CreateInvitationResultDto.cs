namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// RawToken eksponeres KUN her, i det øjeblik invitationen oprettes — bruges til at bygge
/// invitationslinket. Persisteres aldrig; kun TokenHash gemmes på GardenInvitation.
/// </summary>
public sealed record CreateInvitationResultDto(
    Guid InvitationId,
    string RawToken,
    DateTimeOffset ExpiresUtc,
    bool IsFreeSlot);
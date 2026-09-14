namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

public sealed record FreeInvitationQuotaDto(
    int TotalFreeSlots,
    int UsedFreeSlots,
    int RemainingFreeSlots);
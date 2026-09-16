namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

public sealed record SaveCheckoutDraftRequestDto(
    string? UserId,
    string GardenName,
    string? Description,
    GardenAccessLevel Layer,
    AccessCategory Category,
    BillingCycle BillingCycle,
    IReadOnlyDictionary<Guid, int> AddOnQuantities);
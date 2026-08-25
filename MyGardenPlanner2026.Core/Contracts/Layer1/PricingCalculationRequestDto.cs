namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

public sealed record PricingCalculationRequestDto(
    GardenAccessLevel Level,
    AccessCategory AccessCategory,
    BillingCycle BillingCycle,
    int ActiveGardens,
    int ArchivedGardens,
    IReadOnlyDictionary<Guid, int> AddOnQuantities);
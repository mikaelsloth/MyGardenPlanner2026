namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Output fra PricingCalculator's "Fortsæt"-handling: den valgte konfiguration plus det
/// senest beregnede prisresultat, til brug i onboarding-checkout og invitations-accept.
/// </summary>
public sealed record PricingSelectionDto(
    GardenAccessLevel Level,
    AccessCategory Category,
    BillingCycle BillingCycle,
    IReadOnlyDictionary<Guid, int> AddOnQuantities,
    PricingCalculationResultDto PriceResult);
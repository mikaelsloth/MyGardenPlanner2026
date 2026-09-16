namespace MyGardenPlanner2026.Core.Contracts.Onboarding;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

public sealed record AcceptInvitationRequestDto(
    string RawToken,
    string UserId,
    GardenAccessLevel GrantedLayer,
    AccessCategory GrantedCategory,
    BillingCycle? UpgradeBillingCycle,
    IReadOnlyDictionary<Guid, int> AddOnQuantities);
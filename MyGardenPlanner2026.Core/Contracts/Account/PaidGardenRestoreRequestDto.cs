namespace MyGardenPlanner2026.Core.Contracts.Account;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Gendannelse af en slettet have som del af et betalt køb (R10.1d). <paramref name="Email"/> skal
/// være brugerens bekræftede e-mail. Layer/Category/BillingCycle/tilkøb er det, brugeren har købt.
/// </summary>
public sealed record PaidGardenRestoreRequestDto(
    string UserId,
    string Email,
    Guid RestorePendingId,
    GardenAccessLevel Layer,
    AccessCategory Category,
    BillingCycle BillingCycle,
    IReadOnlyDictionary<Guid, int> AddOnQuantities);
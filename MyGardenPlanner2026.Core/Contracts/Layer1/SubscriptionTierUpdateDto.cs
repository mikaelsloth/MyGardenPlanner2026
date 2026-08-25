namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed record SubscriptionTierUpdateDto(
    Guid Id,
    decimal AnnualPrice,
    decimal MonthlyPrice,
    decimal PerpetualPrice);
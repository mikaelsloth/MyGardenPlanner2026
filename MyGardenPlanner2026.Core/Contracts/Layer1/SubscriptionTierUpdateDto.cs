namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed record SubscriptionTierUpdateDto(
    int Id,
    decimal AnnualPrice,
    decimal MonthlyPrice,
    decimal PerpetualPrice);
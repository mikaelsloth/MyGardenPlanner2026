namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

public sealed record SubscriptionTierAdminDto(
    int Id,
    GardenAccessLevel Level,
    AccessCategory AccessCategory,
    string Name,
    decimal AnnualPrice,
    decimal MonthlyPrice,
    decimal PerpetualPrice);
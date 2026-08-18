namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

public sealed record SubscriptionAddOnDto(
    int Id,
    AddOnType Type,
    string Name,
    string UnitDescription,
    decimal AnnualPrice,
    decimal MonthlyPrice);
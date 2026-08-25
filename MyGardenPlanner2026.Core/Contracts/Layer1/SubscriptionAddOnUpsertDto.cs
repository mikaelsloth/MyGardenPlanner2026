namespace MyGardenPlanner2026.Core.Contracts.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

public sealed record SubscriptionAddOnUpsertDto(
    Guid? Id,
    AddOnType Type,
    string Name,
    string UnitDescription,
    decimal AnnualPrice,
    decimal MonthlyPrice,
    decimal PerpetualPrice);
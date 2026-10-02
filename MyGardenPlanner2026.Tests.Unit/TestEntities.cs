namespace MyGardenPlanner2026.Tests.Unit;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Factory-metoder til entiteter, der bygges ens i mange tests. Standardværdierne er
/// valgt, så tests kun behøver at angive det, som de faktisk asserter på.
/// </summary>
internal static class TestEntities
{
    public static SubscriptionAddOn AddOn(
        AddOnType type = AddOnType.ArtefaktpakkeA,
        string name = "Test add-on",
        decimal annualPrice = 48m,
        decimal monthlyPrice = 4m,
        decimal perpetualPrice = 120m) => new()
        {
            Type = type,
            Name = name,
            UnitDescription = "Enhed",
            AnnualPrice = annualPrice,
            MonthlyPrice = monthlyPrice,
            PerpetualPrice = perpetualPrice
        };

    public static SubscriptionTier Tier(
        GardenAccessLevel level = GardenAccessLevel.HaveArkitekt,
        AccessCategory accessCategory = AccessCategory.Viewer,
        string name = "Test tier",
        decimal annualPrice = 100m,
        decimal monthlyPrice = 10m,
        decimal perpetualPrice = 250m) => new()
        {
            Level = level,
            AccessCategory = accessCategory,
            Name = name,
            Description = "Test",
            AnnualPrice = annualPrice,
            MonthlyPrice = monthlyPrice,
            PerpetualPrice = perpetualPrice
        };
}
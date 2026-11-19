namespace MyGardenPlanner2026.Core.Contracts.Account;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Afgør, om et nyt køb dækker en slettet haves tidligere ejerniveau (R10.1d). Jf.
/// GardenAccessLevel: lavere numerisk værdi = højere adgangsniveau. Købet dækker, når Layer er
/// numerisk lig med eller lavere end det krævede, og Category er lig med eller højere.
/// </summary>
public static class GardenRestoreCoverage
{
    public static bool Covers(
        GardenAccessLevel purchasedLayer, AccessCategory purchasedCategory,
        GardenAccessLevel requiredLayer, AccessCategory requiredCategory) =>
        (int)purchasedLayer <= (int)requiredLayer && (int)purchasedCategory >= (int)requiredCategory;
}
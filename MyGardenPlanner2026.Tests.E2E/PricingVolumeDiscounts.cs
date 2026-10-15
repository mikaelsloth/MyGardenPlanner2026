namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>Page-object for rabattabellen (GardenVolumeDiscountTable) på /pricing.</summary>
public static class PricingVolumeDiscounts
{
    /// <summary>Procent-cellen ("85 % af basispris") for en række, fx "2 - 5 haver".</summary>
    public static ILocator PercentCell(IPage page, string rangeLabel) =>
        page.Locator(".garden-volume-discount-table tbody tr")
            .Filter(new() { Has = page.GetByRole(AriaRole.Rowheader, new() { Name = rangeLabel, Exact = true }) })
            .Locator("td");

    public static Task ExpectPercentAsync(IPage page, string rangeLabel, decimal expectedPercent) =>
        PriceAssertions.ExpectValueAsync(
            PercentCell(page, rangeLabel),
            expectedPercent,
            AmountParser.ParseNumber,
            $"rabatten for '{rangeLabel}'");
}
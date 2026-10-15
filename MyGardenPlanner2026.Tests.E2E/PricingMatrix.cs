namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Page-object for matrixen "Basisabonnement pr. bruger" (PricingMatrixTable) på /pricing.
/// Navngivet PricingMatrix for ikke at blive forvekslet med appens PricingPage-komponent.
/// </summary>
public static class PricingMatrix
{
    public const string Title = "Priser & Abonnement – MyGardenPlanner";
    public const string Heading = "Priser & Abonnement";

    /// <summary>Åbner /pricing i en ny kontekst uden login (anonym bruger).</summary>
    public static async Task<IPage> OpenAnonymousAsync(PlaywrightAppFixture fixture, int width)
    {
        var page = await fixture.NewPageAsync(width);
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Pricing}");

        await SmokePageAssertions.ExpectPageAsync(page, Title, Heading);

        return page;
    }

    /// <summary>Vælger cyklus-fanen og venter, til matrixen er indlæst (skeleton væk).</summary>
    public static async Task SelectCycleAsync(IPage page, BillingCycle cycle)
    {
        await TabNavigation.SelectAsync(page, CycleLabel(cycle));
        await Assertions.Expect(page.Locator(".pricing-matrix-table-wrapper [class*='skeleton']"))
            .ToHaveCountAsync(0);
    }

    /// <summary>Prisen (td) for en kategori under lagets overskrift (h3).</summary>
    public static ILocator PriceCell(IPage page, GardenAccessLevel level, AccessCategory category) =>
        page.Locator(".pricing-matrix-table-wrapper").Locator(
            $"xpath=.//h3[normalize-space()='{level.ToDisplayName()}']" +
            $"/following-sibling::div[1]//tr[normalize-space(th)='{category}']/td");

    public static Task ExpectPriceAsync(
        IPage page, GardenAccessLevel level, AccessCategory category, decimal expected) =>
        PriceAssertions.ExpectAmountAsync(
            PriceCell(page, level, category), expected, $"prisen for {level} · {category}");

    private static string CycleLabel(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Annual => "Årligt",
        BillingCycle.Monthly => "Månedligt",
        BillingCycle.Perpetual => "Engangsbeløb",
        _ => throw new ArgumentOutOfRangeException(nameof(cycle), cycle, "Ukendt betalingscyklus."),
    };
}
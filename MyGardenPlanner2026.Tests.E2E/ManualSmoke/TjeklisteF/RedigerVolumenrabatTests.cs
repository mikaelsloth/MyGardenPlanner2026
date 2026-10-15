namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteF;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste F2: rediger volumenrabat. Ny SystemAdmin ændrer prisfaktoren på to trin (2-5 haver og
/// 6-10 haver) på /admin/subscriptions, én række ad gangen, med step-up ved første gem. En anonym bruger
/// ser den nye rabat i pricing-calculatoren (rabattabellen på /pricing læser standardkataloget og ikke
/// databasen, så den kan ikke bruges). Trinene deles med A2 og gendannes altid i finally via en ny circuit.
/// OldMultiplier er seed (Prismatrix tabel 2) og kontrolleres før redigering sammen med Min/Max, så en
/// ugendannet tidligere kørsel eller forskudt række opdages. Skærmbilleder: Volumenrabat_AdminSubscriptions_Gem
/// og Volumenrabat_Pricing_Efter (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class RedigerVolumenrabatTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste F";
    private const string TestName = "Volumenrabat";

    private sealed record DiscountChange(
        int RowIndex, int MinGardens, int? MaxGardens, decimal OldMultiplier, decimal NewMultiplier);

    // Rækker sorteres efter MinGardens: 0 = 1 have, 1 = 2-5, 2 = 6-10.
    private static readonly DiscountChange[] Changes =
    [
        new(RowIndex: 1, MinGardens: 2, MaxGardens: 5, OldMultiplier: 0.90m, NewMultiplier: 0.85m),
        new(RowIndex: 2, MinGardens: 6, MaxGardens: 10, OldMultiplier: 0.80m, NewMultiplier: 0.75m),
    ];

    // HaveArkitekt · Editor · Annual (168,00 pr. have), 0 arkiverede, ingen tilkøb.
    private sealed record CalculatorCheck(int ActiveGardens, decimal DiscountPercent, decimal Subtotal);

    private static readonly CalculatorCheck[] Checks =
    [
        new(ActiveGardens: 5, DiscountPercent: 85m, Subtotal: 714.00m),   // 168 × 0,85 × 5
        new(ActiveGardens: 6, DiscountPercent: 75m, Subtotal: 756.00m),   // 168 × 0,75 × 6
    ];

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task RedigerVolumenrabat_MedStepUp_ViserNyRabatIPricingCalculatorOgGendannes(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var page = await AdminVolumeDiscountsPage.OpenAsync(fixture, user, width);

        foreach (var change in Changes)
        {
            (await AdminVolumeDiscountsPage.ReadMinAsync(page, change.RowIndex))
                .Should().Be(change.MinGardens, "række {0} skal være trinnet fra {1} haver", change.RowIndex, change.MinGardens);
            (await AdminVolumeDiscountsPage.ReadMaxAsync(page, change.RowIndex))
                .Should().Be(change.MaxGardens);
            (await AdminVolumeDiscountsPage.ReadMultiplierAsync(page, change.RowIndex))
                .Should().Be(
                    change.OldMultiplier,
                    "trinnet fra {0} haver skal have seed-faktoren før testen; er den ændret, blev en tidligere kørsel ikke gendannet",
                    change.MinGardens);
        }

        try
        {
            // Gem er pr. række, og et gem nulstiller ugemte rækker: redigér og gem én ad gangen.
            foreach (var change in Changes)
            {
                await AdminVolumeDiscountsPage.SetMultiplierAsync(page, change.RowIndex, change.NewMultiplier);
                await AdminVolumeDiscountsPage.SaveTierAsync(page, user, change.RowIndex, change.MinGardens);
            }

            await Assertions.Expect(AdminVolumeDiscountsPage.Error(page)).ToHaveCountAsync(0);
            await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AdminSubscriptions_Gem");

            // Anonym bruger: calculatoren bruger de redigerede trin.
            var pricing = await PricingMatrix.OpenAnonymousAsync(fixture, width);
            await PricingCalculatorPage.WaitUntilInteractiveAsync(pricing);

            foreach (var check in Checks)
            {
                await PricingCalculatorPage.CalculateAsync(
                    pricing, GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, BillingCycle.Annual,
                    check.ActiveGardens);

                await PricingCalculatorPage.ExpectResultAsync(
                    pricing,
                    weighted: check.ActiveGardens,
                    discountPercent: check.DiscountPercent,
                    gardenSubtotal: check.Subtotal,
                    addOnsTotal: 0m,
                    total: check.Subtotal);
            }

            await ScreenshotRecorder.CaptureAsync(pricing, Chapter, Checklist, TestName, "Pricing_Efter");
        }
        finally
        {
            await RestoreAsync(user, width);
        }
    }

    /// <summary>Ny circuit (ny step-up): skriver seed-faktorerne tilbage og kontrollerer dem.</summary>
    private async Task RestoreAsync(SmokeTestUser user, int width)
    {
        var page = await AdminVolumeDiscountsPage.OpenAsync(fixture, user, width);

        foreach (var change in Changes)
        {
            await AdminVolumeDiscountsPage.SetMultiplierAsync(page, change.RowIndex, change.OldMultiplier);
            await AdminVolumeDiscountsPage.SaveTierAsync(page, user, change.RowIndex, change.MinGardens);
        }

        foreach (var change in Changes)
        {
            (await AdminVolumeDiscountsPage.ReadMultiplierAsync(page, change.RowIndex))
                .Should().Be(change.OldMultiplier);
        }
    }
}
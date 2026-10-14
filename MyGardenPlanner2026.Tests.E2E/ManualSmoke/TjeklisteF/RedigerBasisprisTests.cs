namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteF;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste F1: rediger basispris. Ny SystemAdmin redigerer tre priser (tre lag, tre cyklusser) på
/// /admin/subscriptions, én række ad gangen (Gem er pr. række), med step-up ved første gem. En anonym
/// bruger ser de nye priser på /pricing. Priserne deles med A2 (PricingCalculatorTests) og gendannes
/// altid i finally via en ny circuit (ny step-up). OldPrice er seed (Prismatrix tabel 1) og kontrolleres
/// før redigering, så en ugendannet tidligere kørsel opdages. Skærmbilleder: Basispris_AdminSubscriptions_Gem
/// og Basispris_Pricing_Efter (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class RedigerBasisprisTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste F";
    private const string TestName = "Basispris";

    private sealed record PriceChange(
        GardenAccessLevel Level,
        AccessCategory Category,
        BillingCycle Cycle,
        decimal OldPrice,
        decimal NewPrice);

    private static readonly PriceChange[] Changes =
    [
        new(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, BillingCycle.Annual, 168.00m, 175.00m),
        new(GardenAccessLevel.BedDesigner, AccessCategory.Administrator, BillingCycle.Monthly, 20.00m, 22.50m),
        new(GardenAccessLevel.Planlaegger, AccessCategory.Viewer, BillingCycle.Perpetual, 60.00m, 65.00m),
    ];

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task RedigerBasispris_MedStepUp_ViserNyePriserPaaPricingOgGendannes(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var page = await AdminSubscriptionsPage.OpenAsync(fixture, user, width);

        foreach (var change in Changes)
        {
            var current = await AdminSubscriptionsPage.ReadPriceAsync(
                page, change.Cycle, change.Level, change.Category);

            current.Should().Be(
                change.OldPrice,
                "{0} · {1} ({2}) skal have seed-prisen før testen; er den ændret, blev en tidligere kørsel ikke gendannet",
                change.Level, change.Category, change.Cycle);
        }

        try
        {
            // Gem er pr. række, og et gem nulstiller ugemte rækker: redigér og gem én ad gangen.
            foreach (var change in Changes)
            {
                await AdminSubscriptionsPage.SetPriceAsync(
                    page, change.Cycle, change.Level, change.Category, change.NewPrice);
                await AdminSubscriptionsPage.SaveTierAsync(page, user, change.Level, change.Category);
            }

            await Assertions.Expect(AdminSubscriptionsPage.Error(page)).ToHaveCountAsync(0);
            await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AdminSubscriptions_Gem");

            // Anonym bruger: nye priser på /pricing, pr. cyklus.
            var pricing = await PricingMatrix.OpenAnonymousAsync(fixture, width);

            foreach (var change in Changes)
            {
                await PricingMatrix.SelectCycleAsync(pricing, change.Cycle);
                await PricingMatrix.ExpectPriceAsync(pricing, change.Level, change.Category, change.NewPrice);
            }

            await ScreenshotRecorder.CaptureAsync(pricing, Chapter, Checklist, TestName, "Pricing_Efter");
        }
        finally
        {
            await RestoreAsync(user, width);
        }
    }

    /// <summary>Ny circuit (ny step-up): skriver seed-priserne tilbage og kontrollerer dem.</summary>
    private async Task RestoreAsync(SmokeTestUser user, int width)
    {
        var page = await AdminSubscriptionsPage.OpenAsync(fixture, user, width);

        foreach (var change in Changes)
        {
            await AdminSubscriptionsPage.SetPriceAsync(
                page, change.Cycle, change.Level, change.Category, change.OldPrice);
            await AdminSubscriptionsPage.SaveTierAsync(page, user, change.Level, change.Category);
        }

        foreach (var change in Changes)
        {
            (await AdminSubscriptionsPage.ReadPriceAsync(page, change.Cycle, change.Level, change.Category))
                .Should().Be(change.OldPrice);
        }
    }
}
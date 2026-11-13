namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteA;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste A2: pricing-calculatoren beregner korrekt ifølge Prismatrix (tabel 1-3).
/// Forventede værdier er faste tal (ikke udledt af produktionskoden). Tilføj et scenarie
/// ved at tilføje en række i <see cref="Scenarios"/>.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class PricingCalculatorTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste A";
    private const string ScreenshotTestName = "Prisberegning";
    private const string ScreenshotScenarioPrefix = "S3";

    private sealed record PricingScenario(
        string Name,
        GardenAccessLevel Level,
        AccessCategory Category,
        BillingCycle Cycle,
        int Active,
        int Archived,
        AddOnQuantity[] AddOns,
        decimal Weighted,
        decimal DiscountPercent,
        decimal GardenSubtotal,
        decimal AddOnsTotal,
        decimal Total);

    private static readonly PricingScenario[] Scenarios =
    [
        new("S1 Basis uden rabat",
            GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, BillingCycle.Annual,
            Active: 1, Archived: 0, AddOns: [],
            Weighted: 1m, DiscountPercent: 100m, GardenSubtotal: 168.00m, AddOnsTotal: 0m, Total: 168.00m),

        new("S2 Administrator: arkiverede vejer 0,25",
            GardenAccessLevel.BedDesigner, AccessCategory.Administrator, BillingCycle.Monthly,
            Active: 3, Archived: 4, AddOns: [new(AddOnType.BedforslagNiveau2, 1)],
            Weighted: 4m, DiscountPercent: 90m, GardenSubtotal: 72.00m, AddOnsTotal: 15.00m, Total: 87.00m),

        new("S3 Øvrige: arkiverede vejer 1,0",
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer, BillingCycle.Perpetual,
            Active: 5, Archived: 1,
            AddOns: [new(AddOnType.ArtefaktpakkeA, 2), new(AddOnType.ArtefaktpakkeB, 1)],
            Weighted: 6m, DiscountPercent: 80m, GardenSubtotal: 288.00m, AddOnsTotal: 300.00m, Total: 588.00m),

        new("S4 Trappegrænse 11 haver",
            GardenAccessLevel.HaveArkitekt, AccessCategory.ViewerPlus, BillingCycle.Annual,
            Active: 10, Archived: 1,
            AddOns: [new(AddOnType.BedeINiveau2, 2), new(AddOnType.PlanlagteBedeNiveau3, 1)],
            Weighted: 11m, DiscountPercent: 70m, GardenSubtotal: 646.80m, AddOnsTotal: 324.00m, Total: 970.80m),

        new("S5 Administrator: vægtet 198 giver 50 pct.",
            GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, BillingCycle.Monthly,
            Active: 197, Archived: 4, AddOns: [],
            Weighted: 198m, DiscountPercent: 50m, GardenSubtotal: 2772.00m, AddOnsTotal: 0m, Total: 2772.00m),
    ];

    public static TheoryData<string> Cases()
    {
        var data = new TheoryData<string>();
        foreach (var scenario in Scenarios)
        {
            data.Add(scenario.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Pricingcalculator_Scenarie_BeregnerIfoelgePrismatrix(string scenarioName)
    {
        var scenario = Scenarios.Single(s => s.Name == scenarioName);
        var page = await OpenPricingAsync(Viewports.Desktop);

        await ApplyScenarioAsync(page, scenario);

        await ExpectResultAsync(page, scenario);
    }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Pricingcalculator_ValgtScenarie_GemmerSkaermbillede(int width)
    {
        var scenario = Scenarios.Single(s => s.Name.StartsWith(ScreenshotScenarioPrefix, StringComparison.Ordinal));
        var page = await OpenPricingAsync(width);

        await ApplyScenarioAsync(page, scenario);
        await ExpectResultAsync(page, scenario);

        await ScreenshotRecorder.CaptureAsync(
            page, Chapter, Checklist, ScreenshotTestName, "PricingCalculator");
    }

    private async Task<IPage> OpenPricingAsync(int width)
    {
        var page = await PricingMatrix.OpenAnonymousAsync(fixture, width);

        // Tilkøb hentes fra databasen: mangler de, er kataloget ikke seedet.
        var expectedAddOns = new DefaultSubscriptionAddOnCatalog().GetDefaultAddOns().Count;
        await Assertions.Expect(PricingCalculatorPage.Calculator(page).Locator("input[id^='addon-']"))
            .ToHaveCountAsync(expectedAddOns);

        return page;
    }

    private static async Task ApplyScenarioAsync(IPage page, PricingScenario scenario)
    {
        await PricingCalculatorPage.WaitUntilInteractiveAsync(page);

        await PricingCalculatorPage.CalculateAsync(
            page,
            scenario.Level,
            scenario.Category,
            scenario.Cycle,
            scenario.Active,
            scenario.Archived,
            scenario.AddOns);
    }

    private static Task ExpectResultAsync(IPage page, PricingScenario scenario) =>
        PricingCalculatorPage.ExpectResultAsync(
            page,
            scenario.Weighted,
            scenario.DiscountPercent,
            scenario.GardenSubtotal,
            scenario.AddOnsTotal,
            scenario.Total);
}
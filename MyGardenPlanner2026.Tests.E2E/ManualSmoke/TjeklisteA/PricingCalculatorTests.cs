namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteA;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Globalization;

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

    private sealed record AddOnQuantity(AddOnType Type, int Quantity);

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
        var page = await fixture.NewPageAsync(width);
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Pricing}");

        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);

        // Tilkøb hentes fra databasen: mangler de, er kataloget ikke seedet.
        var expectedAddOns = new DefaultSubscriptionAddOnCatalog().GetDefaultAddOns().Count;
        await Assertions.Expect(Calculator(page).Locator("input[id^='addon-']")).ToHaveCountAsync(expectedAddOns);

        return page;
    }

    private static ILocator Calculator(IPage page) => page.Locator(".pricing-calculator");

    private static ILocator CalculateButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Beregn pris" });

    private static ILocator ResultValue(IPage page, string label) =>
        Calculator(page).Locator(".summary-card")
            .Filter(new() { Has = page.GetByText(label, new() { Exact = true }) })
            .Locator(".summary-card-value");

    private static async Task ApplyScenarioAsync(IPage page, PricingScenario scenario)
    {
        // Uden et forbundet kredsløb mistes input. Standardberegningen bruges som "klar"-signal.
        await BlazorInteraction.ActUntilAsync(
            () => CalculateButton(page).ClickAsync(),
            async () => await Calculator(page).Locator(".pricing-calculator-result").CountAsync() == 1,
            description: "Pricing-calculatoren blev ikke interaktiv eller viste intet resultat");

        var calculator = Calculator(page);

        await calculator.Locator("#calc-level").SelectOptionAsync(scenario.Level.ToString());
        await calculator.Locator("#calc-category").SelectOptionAsync(scenario.Category.ToString());
        await calculator.Locator("#calc-cycle").SelectOptionAsync(scenario.Cycle.ToString());

        await SetNumberAsync(calculator.Locator("#calc-active"), scenario.Active);
        await SetNumberAsync(calculator.Locator("#calc-archived"), scenario.Archived);

        var addOnNames = new DefaultSubscriptionAddOnCatalog().GetDefaultAddOns()
            .ToDictionary(a => a.Type, a => a.Name);

        foreach (var addOn in scenario.AddOns)
        {
            var input = calculator.GetByLabel(addOnNames[addOn.Type], new() { Exact = true });
            await SetNumberAsync(input, addOn.Quantity);
        }

        await CalculateButton(page).ClickAsync();
    }

    private static async Task SetNumberAsync(ILocator input, int value)
    {
        await input.FillAsync(value.ToString(CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @bind/@onchange udløses først ved blur
    }

    private static async Task ExpectResultAsync(IPage page, PricingScenario scenario)
    {
        await ExpectValueAsync(page, "Total", scenario.Total, ParseAmount);
        await ExpectValueAsync(page, "Vægtet antal haver", scenario.Weighted, ParseNumber);
        await ExpectValueAsync(page, "Rabat-trappe", scenario.DiscountPercent, ParseNumber);
        await ExpectValueAsync(page, "Have-subtotal", scenario.GardenSubtotal, ParseAmount);
        await ExpectValueAsync(page, "Tilkøb i alt", scenario.AddOnsTotal, ParseAmount);
    }

    private static async Task ExpectValueAsync(
        IPage page, string label, decimal expected, Func<string, decimal?> parse)
    {
        var value = ResultValue(page, label);
        string? lastText = null;

        var matched = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            try
            {
                lastText = await value.InnerTextAsync(new() { Timeout = 1000 });
                return parse(lastText) == expected;
            }
            catch (PlaywrightException)
            {
                return false; // resultatet er midlertidigt væk under genberegning
            }
        }, TimeSpan.FromSeconds(5));

        matched.Should().BeTrue($"'{label}' skulle være {expected}, men var '{lastText}'");
    }

    // Beløb vises med C2 (to decimaler). Cifrene sammenlignes, så testen er uafhængig af
    // kultur, valutategn, tusindtalsseparator og ikke-brydende mellemrum.
    private static decimal? ParseAmount(string text)
    {
        var digits = new string([.. text.Where(char.IsDigit)]);
        return digits.Length == 0
            ? null
            : decimal.Parse(digits, CultureInfo.InvariantCulture) / 100m;
    }

    // Heltal, brøk (fx "4,25") og procent ("90 %").
    private static decimal? ParseNumber(string text)
    {
        var cleaned = new string([.. text.Where(c => char.IsDigit(c) || c is ',' or '.')])
            .Replace(',', '.');

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
    }
}
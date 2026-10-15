namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

/// <summary>
/// Page-object for .pricing-calculator på /pricing (uden tilkøb). Samme selektorer og afventning som
/// PricingCalculatorTests (A2), som endnu har sin egen kopi (kandidat til dedup).
/// </summary>
public static class PricingCalculatorPage
{
    public static ILocator Calculator(IPage page) => page.Locator(".pricing-calculator");

    public static ILocator CalculateButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Beregn pris" });

    public static ILocator ResultValue(IPage page, string label) =>
        Calculator(page).Locator(".summary-card")
            .Filter(new() { Has = page.GetByText(label, new() { Exact = true }) })
            .Locator(".summary-card-value");

    /// <summary>Uden et forbundet kredsløb mistes input. Standardberegningen bruges som "klar"-signal.</summary>
    public static Task WaitUntilInteractiveAsync(IPage page) =>
        BlazorInteraction.ActUntilAsync(
            () => CalculateButton(page).ClickAsync(),
            async () => await Calculator(page).Locator(".pricing-calculator-result").CountAsync() == 1,
            description: "Pricing-calculatoren blev ikke interaktiv eller viste intet resultat");

    /// <summary>Udfylder og klikker "Beregn pris". Venter ikke på resultatet (brug ExpectResultAsync).</summary>
    public static async Task CalculateAsync(
        IPage page,
        GardenAccessLevel level,
        AccessCategory category,
        BillingCycle cycle,
        int activeGardens,
        int archivedGardens = 0)
    {
        var calculator = Calculator(page);

        await calculator.Locator("#calc-level").SelectOptionAsync(level.ToString());
        await calculator.Locator("#calc-category").SelectOptionAsync(category.ToString());
        await calculator.Locator("#calc-cycle").SelectOptionAsync(cycle.ToString());

        await SetNumberAsync(calculator.Locator("#calc-active"), activeGardens);
        await SetNumberAsync(calculator.Locator("#calc-archived"), archivedGardens);

        await CalculateButton(page).ClickAsync();
    }

    public static async Task ExpectResultAsync(
        IPage page,
        decimal weighted,
        decimal discountPercent,
        decimal gardenSubtotal,
        decimal addOnsTotal,
        decimal total)
    {
        await ExpectValueAsync(page, "Total", total, AmountParser.Parse);
        await ExpectValueAsync(page, "Vægtet antal haver", weighted, AmountParser.ParseNumber);
        await ExpectValueAsync(page, "Rabat-trappe", discountPercent, AmountParser.ParseNumber);
        await ExpectValueAsync(page, "Have-subtotal", gardenSubtotal, AmountParser.Parse);
        await ExpectValueAsync(page, "Tilkøb i alt", addOnsTotal, AmountParser.Parse);
    }

    private static async Task SetNumberAsync(ILocator input, int value)
    {
        await input.FillAsync(value.ToString(CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @bind/@onchange udløses først ved blur
    }

    private static Task ExpectValueAsync(
        IPage page, string label, decimal expected, Func<string, decimal?> parse) =>
        PriceAssertions.ExpectValueAsync(ResultValue(page, label), expected, parse, $"'{label}'");
}
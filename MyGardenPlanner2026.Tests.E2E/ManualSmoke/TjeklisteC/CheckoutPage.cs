namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Selektorer og handlinger på /onboarding/checkout (InteractiveServer). Havenavnet indtastes med ActUntilAsync,
/// fordi prerendering og kredsløbsstart kan nulstille indtastninger.
/// </summary>
internal static class CheckoutPage
{
    public const string CheckoutTitle = "Opret betalt have – MyGardenPlanner";
    public const string CheckoutHeading = "Opret betalt have";
    public const string ContinueButtonName = "Fortsæt";
    public const string ChangeSubscriptionButtonName = "Ændr abonnement";
    public const string NewGardenRadioName = "Opret en ny have";
    public const string RestoreConfirmLabel = "Gennemfør betaling og gendan have";
    public const string CreateConfirmLabel = "Gennemfør betaling og opret have";
    public const string ConfigurationStep = "1. Konfiguration";
    public const string PaymentStep = "3. Betaling";

    public static async Task OpenAsync(PlaywrightAppFixture fixture, IPage page)
    {
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.OnboardingCheckout}");
        await SmokePageAssertions.ExpectPageAsync(page, CheckoutTitle, CheckoutHeading);
    }

    /// <summary>Åbner welcome-siden, klikker "Vælg abonnement og gendan" og venter på checkout.</summary>
    public static async Task OpenViaWelcomeLinkAsync(PlaywrightAppFixture fixture, IPage page)
    {
        await WelcomePage.OpenAsync(fixture, page);
        await WelcomePage.RestoreLink(page).ClickAsync();
        await SmokePageAssertions.ExpectPageAsync(page, CheckoutTitle, CheckoutHeading);
    }

    public static ILocator ActiveStep(IPage page) => page.Locator(".onboarding-steps .context-tab.active");

    public static ILocator GardenNameInput(IPage page) => page.Locator("#garden-name");

    public static ILocator RestoreChoice(IPage page) => page.Locator(".restore-garden-choice");

    public static ILocator PaymentPage(IPage page) => page.Locator(".payment-mock-page");

    public static ILocator RestoreRadio(IPage page, string gardenName) =>
        page.GetByRole(AriaRole.Radio, new() { Name = $"Gendan {gardenName}" });

    public static ILocator NewGardenRadio(IPage page) =>
        page.GetByRole(AriaRole.Radio, new() { Name = NewGardenRadioName, Exact = true });

    public static ILocator ChangeSubscriptionButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = ChangeSubscriptionButtonName, Exact = true });

    public static ILocator ConfirmButton(IPage page, string label) =>
        page.GetByRole(AriaRole.Button, new() { Name = label, Exact = true });

    /// <summary>Indtaster havenavnet, til prisberegneren vises (venter på forbundet kredsløb).</summary>
    public static Task EnterGardenNameAsync(IPage page, string name) =>
        BlazorInteraction.ActUntilAsync(
            async () =>
            {
                await GardenNameInput(page).FillAsync(name);
                await GardenNameInput(page).BlurAsync();
            },
            async () => await PricingCalculatorPage.Calculator(page).CountAsync() == 1,
            description: "Prisberegneren blev ikke vist efter indtastning af havenavn");

    /// <summary>Vælger niveau og kategori, klikker "Fortsæt" og venter på betalingstrinnet.</summary>
    public static async Task ChooseSubscriptionAsync(IPage page, GardenAccessLevel level, AccessCategory category)
    {
        var calculator = PricingCalculatorPage.Calculator(page);
        await calculator.Locator("#calc-level").SelectOptionAsync(level.ToString());
        await calculator.Locator("#calc-category").SelectOptionAsync(category.ToString());
        await calculator.GetByRole(AriaRole.Button, new() { Name = ContinueButtonName, Exact = true }).ClickAsync();

        await Assertions.Expect(ActiveStep(page)).ToHaveTextAsync(PaymentStep);
    }

    public static async Task ConfigureAsync(
        IPage page, string gardenName, GardenAccessLevel level, AccessCategory category)
    {
        await EnterGardenNameAsync(page, gardenName);
        await ChooseSubscriptionAsync(page, level, category);
    }
}
namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using Microsoft.Playwright;

/// <summary>Tekster og handlinger på /onboarding/welcome (Tjekliste C6/C7).</summary>
internal static class WelcomePage
{
    public const string WelcomeTitle = "Kom i gang – MyGardenPlanner";
    public const string WelcomeHeading = "Velkommen til MyGardenPlanner";
    public const string RestoreBannerText = "Du har gemte haver, der kan gendannes";
    public const string RestoreLinkName = "Vælg abonnement og gendan";
    public const string SandboxButtonName = "Opret gratis prøveperiode";

    public static async Task OpenAsync(PlaywrightAppFixture fixture, IPage page)
    {
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.OnboardingWelcome}");
        await SmokePageAssertions.ExpectPageAsync(page, WelcomeTitle, WelcomeHeading);
    }

    public static ILocator RestoreLink(IPage page) =>
        page.GetByRole(AriaRole.Link, new() { Name = RestoreLinkName, Exact = true });
}
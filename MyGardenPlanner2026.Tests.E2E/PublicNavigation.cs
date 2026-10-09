namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// Navigation i den offentlige header. Linket "Opret bruger" klikkes direkte i headeren.
/// Er det ikke synligt (mobilbredde), åbnes NavDrawer først med samme mønster som
/// MobilmenuTests (Blazor ignorerer klik, til kredsløbet er forbundet).
/// </summary>
public static class PublicNavigation
{
    private const string RegisterLink = "a[href='/account/register' i]";

    public static async Task ClickRegisterAsync(IPage page)
    {
        var headerLink = page.Locator($"header.public-header {RegisterLink}");
        if (await headerLink.IsVisibleAsync())
        {
            await headerLink.ClickAsync();
            return;
        }

        var hamburger = page.Locator("header.public-header button[aria-label='Åbn menu']");
        var openDrawer = page.Locator("nav.nav-drawer.open");

        await BlazorInteraction.ActUntilAsync(
            () => hamburger.ClickAsync(),
            async () => await openDrawer.CountAsync() == 1,
            description: "Drawer åbnede ikke");

        await openDrawer.Locator(RegisterLink).ClickAsync();
    }
}
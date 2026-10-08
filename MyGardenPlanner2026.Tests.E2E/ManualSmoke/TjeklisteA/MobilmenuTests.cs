namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteA;

using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste A3: mobilvisning &lt; 940 px. Hamburger + NavDrawer åbner/lukker med mus og
/// tastatur, og fokus-trap virker. Ingen skærmbilleder (jf. tjeklisten).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class MobilmenuTests(PlaywrightAppFixture fixture)
{
    // Design: mobilvisning (hamburger + NavDrawer) under 940 px.
    private const int MobileBreakpoint = 940;

    [GeneratedRegex("^true$", RegexOptions.IgnoreCase)]
    private static partial Regex TrueValue { get; }

    [GeneratedRegex("^false$", RegexOptions.IgnoreCase)]
    private static partial Regex FalseValue { get; }

    private static ILocator Hamburger(IPage page) =>
        page.Locator("header.public-header button[aria-label='Åbn menu']");

    private static ILocator OpenDrawer(IPage page) => page.Locator("nav.nav-drawer.open");

    private static ILocator DrawerWithFocus(IPage page) => page.Locator("nav.nav-drawer:focus-within");

    private static ILocator HeaderForsideLink(IPage page) =>
        page.Locator("header.public-header").GetByRole(AriaRole.Link, new() { Name = "Forside" });

    private async Task<IPage> OpenLandingAsync(int width)
    {
        var page = await fixture.NewPageAsync(width);
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Landing}");
        return page;
    }

    private static Task OpenDrawerAsync(IPage page, Func<Task> trigger) =>
        BlazorInteraction.ActUntilAsync(
            trigger,
            async () => await OpenDrawer(page).CountAsync() == 1,
            description: "Drawer åbnede ikke");

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.BelowBreakpoint)]
    [InlineData(Viewports.AtBreakpoint)]
    [InlineData(Viewports.Desktop)]
    public async Task Breakpoint_VedMobilbredde_ViserHamburgerIStedetForDesktopLinks(int width)
    {
        var page = await OpenLandingAsync(width);

        if (width < MobileBreakpoint)
        {
            await Assertions.Expect(Hamburger(page)).ToBeVisibleAsync();
            await Assertions.Expect(HeaderForsideLink(page)).ToBeHiddenAsync();
        }
        else
        {
            await Assertions.Expect(Hamburger(page)).ToBeHiddenAsync();
            await Assertions.Expect(HeaderForsideLink(page)).ToBeVisibleAsync();
        }
    }

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.BelowBreakpoint)]
    public async Task Hamburger_MedMusklik_AabnerOgLukkerDrawer(int width)
    {
        var page = await OpenLandingAsync(width);

        await OpenDrawerAsync(page, () => Hamburger(page).ClickAsync());

        await Assertions.Expect(Hamburger(page)).ToHaveAttributeAsync("aria-expanded", TrueValue);
        await Assertions.Expect(page.Locator(".drawer-backdrop")).ToHaveCountAsync(1);

        // Klik på backdrop uden for drawer (drawer fylder højst 340 px fra venstre).
        await page.Locator(".drawer-backdrop").ClickAsync(
            new() { Position = new Position { X = width - 20, Y = 400 } });

        await Assertions.Expect(OpenDrawer(page)).ToHaveCountAsync(0);
        await Assertions.Expect(Hamburger(page)).ToHaveAttributeAsync("aria-expanded", FalseValue);
    }

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.BelowBreakpoint)]
    public async Task Hamburger_MedTastatur_AabnerOgLukkerDrawerMedEscape(int width)
    {
        var page = await OpenLandingAsync(width);

        await OpenDrawerAsync(page, async () =>
        {
            await Hamburger(page).FocusAsync();
            await page.Keyboard.PressAsync("Enter");
        });

        await Assertions.Expect(DrawerWithFocus(page)).ToHaveCountAsync(1);

        await page.Keyboard.PressAsync("Escape");

        await Assertions.Expect(OpenDrawer(page)).ToHaveCountAsync(0);
    }

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.BelowBreakpoint)]
    public async Task Drawer_MedFokusTrap_HolderFokusIDrawerVedTabOgShiftTab(int width)
    {
        var page = await OpenLandingAsync(width);

        await OpenDrawerAsync(page, () => Hamburger(page).ClickAsync());
        await Assertions.Expect(DrawerWithFocus(page)).ToHaveCountAsync(1);

        // Drawer har 4 links; 6 tryk sikrer, at fokus må krydse drawerens ender.
        const int presses = 6;

        for (var i = 0; i < presses; i++)
        {
            await page.Keyboard.PressAsync("Tab");
            await Assertions.Expect(DrawerWithFocus(page)).ToHaveCountAsync(1);
        }

        for (var i = 0; i < presses; i++)
        {
            await page.Keyboard.PressAsync("Shift+Tab");
            await Assertions.Expect(DrawerWithFocus(page)).ToHaveCountAsync(1);
        }
    }

    // Tilføjet ud fra designdokumentet (NavDrawer: "Focus trap og restore"), ikke fra tjeklisten.
    [Fact]
    public async Task Drawer_VedLukning_ReturnererFokusTilHamburger()
    {
        var page = await OpenLandingAsync(Viewports.Tablet);

        await OpenDrawerAsync(page, () => Hamburger(page).ClickAsync());
        await Assertions.Expect(DrawerWithFocus(page)).ToHaveCountAsync(1);

        await page.Keyboard.PressAsync("Escape");

        await Assertions.Expect(OpenDrawer(page)).ToHaveCountAsync(0);
        await Assertions.Expect(Hamburger(page)).ToBeFocusedAsync();
    }
}
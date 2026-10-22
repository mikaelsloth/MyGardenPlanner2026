namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using static MyGardenPlanner2026.Tests.E2E.AdminAccessMatrix;

/// <summary>
/// Page-object for AdminNavMenu (nav.admin-nav-menu, aria-label "Administration"). Navigationen ligger i
/// AdminLayout og er statisk: den renderes kun ved selve sideindlæsningen. Links findes via href-suffiks, da
/// AuditLog-linket kan indeholde et .nav-badge, og href er relativ ("admin/subscriptions").
/// </summary>
public static class AdminNavigation
{
    public static ILocator Nav(IPage page) => page.Locator("nav.admin-nav-menu");

    public static ILocator Links(IPage page) => Nav(page).Locator("a");

    public static ILocator Link(IPage page, AdminPage target) =>
        Nav(page).Locator($"a[href$='{target.Route.TrimStart('/')}' i]");

    /// <summary>
    /// Navigationen viser præcis de forventede links. Positiv kontrol først (de forventede links er synlige),
    /// derefter fravær af øvrige links og det samlede antal.
    /// </summary>
    public static async Task ExpectExactLinksAsync(IPage page, IReadOnlyList<AdminPage> expected)
    {
        await Assertions.Expect(Nav(page)).ToBeVisibleAsync();

        foreach (var target in expected)
        {
            await Assertions.Expect(Link(page, target)).ToBeVisibleAsync();
        }

        foreach (var target in AllPages.Where(p => !expected.Contains(p)))
        {
            await Assertions.Expect(Link(page, target)).ToHaveCountAsync(0);
        }

        await Assertions.Expect(Links(page)).ToHaveCountAsync(expected.Count);
    }
}
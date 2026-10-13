namespace MyGardenPlanner2026.Tests.E2E;

using FluentAssertions;
using Microsoft.Playwright;

/// <summary>Fælles assertion for, at en bruger uden adgang er sendt til /Account/AccessDenied.</summary>
public static class AccessAssertions
{
    public const string AccessDeniedTitle = "Adgang nægtet – MyGardenPlanner";
    public const string AccessDeniedHeading = "Adgang nægtet";
    public const string AccessDeniedText = "Du har ikke adgang til denne ressource.";

    public static async Task ExpectAccessDeniedAsync(IPage page)
    {
        page.Url.Should().ContainEquivalentOf(E2ERoutes.AccessDenied);

        await SmokePageAssertions.ExpectPageAsync(page, AccessDeniedTitle, AccessDeniedHeading);
        await Assertions.Expect(page.GetByText(AccessDeniedText)).ToBeVisibleAsync();
    }
}
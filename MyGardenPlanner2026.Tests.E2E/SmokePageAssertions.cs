namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>Fælles sideassertions for manual smoke-tests: titel, præcis én h1, ingen skeletons og skjult fejl-banner.</summary>
public static class SmokePageAssertions
{
    public static async Task ExpectPageAsync(IPage page, string title, string heading)
    {
        await Assertions.Expect(page).ToHaveTitleAsync(title);

        var h1 = page.GetByRole(AriaRole.Heading, new() { Level = 1 });
        await Assertions.Expect(h1).ToHaveCountAsync(1);
        await Assertions.Expect(h1).ToHaveTextAsync(heading);

        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    }
}
namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// Vælger en fane (role=tab) og gentager klikket, til fanen er aktiv (klassen "active").
/// Uden et forbundet kredsløb ignorerer Blazor Server klik.
/// </summary>
public static class TabNavigation
{
    public static Task SelectAsync(IPage page, string name)
    {
        var tab = page.GetByRole(AriaRole.Tab, new() { Name = name, Exact = true });

        return BlazorInteraction.ActUntilAsync(
            () => tab.ClickAsync(),
            async () =>
            {
                var classes = await tab.GetAttributeAsync("class") ?? string.Empty;
                return classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("active");
            },
            maxAttempts: 12,
            description: $"Fanen '{name}' blev ikke valgt");
    }
}
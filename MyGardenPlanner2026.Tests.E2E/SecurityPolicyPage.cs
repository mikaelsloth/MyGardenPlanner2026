namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using System.Globalization;

/// <summary>Page-object for /admin/security-policies (fanerne og fanen AuditLog-eksport).</summary>
public static class SecurityPolicyPage
{
    public const string ExportJobSavedText = "AuditLog-eksportjob-policyen er opdateret og trådt i kraft.";

    public static ILocator Retention(IPage page) => page.Locator("#exportjob-retention");

    public static ILocator SaveButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true });

    /// <summary>Statusbeskeden på siden (ryddes ved fanskift).</summary>
    public static ILocator Success(IPage page) => page.Locator(".status-success");

    /// <summary>
    /// Vælger en fane og gentager klikket, til fanen er aktiv (uden et forbundet kredsløb ignoreres klik).
    /// Aktiv fane har klassen "active"; aria-selected er en bool-attribut uden værdi.
    /// </summary>
    public static Task SelectTabAsync(IPage page, string name)
    {
        var tab = page.GetByRole(AriaRole.Tab, new() { Name = name });

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

    public static async Task OpenExportJobTabAsync(IPage page)
    {
        await SelectTabAsync(page, "AuditLog-eksport");
        await Assertions.Expect(Retention(page)).ToBeVisibleAsync();
    }

    public static async Task<int> ReadRetentionAsync(IPage page) =>
        int.Parse(await Retention(page).InputValueAsync(), CultureInfo.InvariantCulture);

    public static async Task SetRetentionAsync(IPage page, int value)
    {
        var input = Retention(page);
        await input.FillAsync(value.ToString(CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @onchange udløses først ved blur
    }
}
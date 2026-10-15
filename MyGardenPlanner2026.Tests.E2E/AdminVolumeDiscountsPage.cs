namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using System.Globalization;

/// <summary>
/// Page-object for /admin/subscriptions, fanen "Volumen-rabatter" (VolumeDiscountEditor).
/// Rækkerne har ens labels, så de findes via indeks (sorteret efter MinGardens). Gem er pr. række
/// og udløser step-up; et vellykket gem genindlæser alle rækker og nulstiller ugemte indtastninger.
/// </summary>
public static class AdminVolumeDiscountsPage
{
    /// <summary>7 trin i seed (Prismatrix tabel 2).</summary>
    public const int SeededTierCount = 7;

    /// <summary>Logger ind, åbner siden og fanen og venter på rækkerne.</summary>
    public static async Task<IPage> OpenAsync(PlaywrightAppFixture fixture, SmokeTestUser user, int? width = null)
    {
        var page = await AdminSubscriptionsPage.OpenAsync(fixture, user, width);

        await TabNavigation.SelectAsync(page, AdminSubscriptionsPage.VolumeDiscountsTab);
        await Assertions.Expect(TierRows(page)).ToHaveCountAsync(SeededTierCount);

        return page;
    }

    public static ILocator TierRows(IPage page) => page.Locator(".volume-discount-editor tbody tr");

    private static ILocator Row(IPage page, int index) => TierRows(page).Nth(index);

    public static ILocator MinInput(IPage page, int index) => Row(page, index).Locator("input[id^='min-']");

    public static ILocator MaxInput(IPage page, int index) => Row(page, index).Locator("input[id^='max-']");

    public static ILocator MultiplierInput(IPage page, int index) => Row(page, index).Locator("input[id^='mult-']");

    public static ILocator SaveButton(IPage page, int index) =>
        Row(page, index).GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true });

    /// <summary>Succesbesked for en gemt række; MinGardens er rækkens (uændrede) startværdi.</summary>
    public static ILocator SavedMessage(IPage page, int minGardens) =>
        page.Locator(".status-success")
            .Filter(new() { HasText = $"Volumenrabat-trappe fra {minGardens} haver er opdateret." });

    public static ILocator Error(IPage page) => page.Locator(".volume-discount-editor .status-danger");

    public static async Task<int> ReadMinAsync(IPage page, int index) =>
        int.Parse(await MinInput(page, index).InputValueAsync(), CultureInfo.InvariantCulture);

    /// <summary>Null = åben trappe (tomt felt).</summary>
    public static async Task<int?> ReadMaxAsync(IPage page, int index)
    {
        var text = await MaxInput(page, index).InputValueAsync();
        return string.IsNullOrWhiteSpace(text) ? null : int.Parse(text, CultureInfo.InvariantCulture);
    }

    public static async Task<decimal> ReadMultiplierAsync(IPage page, int index) =>
        decimal.Parse(await MultiplierInput(page, index).InputValueAsync(), CultureInfo.InvariantCulture);

    public static async Task SetMultiplierAsync(IPage page, int index, decimal value)
    {
        var input = MultiplierInput(page, index);
        await input.FillAsync(value.ToString("0.00", CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @onchange udløses først ved blur
    }

    public static Task SaveTierAsync(IPage page, SmokeTestUser user, int index, int minGardens) =>
        AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            SaveButton(page, index),
            SavedMessage(page, minGardens),
            Error(page),
            $"Gem af volumenrabat-trappen fra {minGardens} haver");
}
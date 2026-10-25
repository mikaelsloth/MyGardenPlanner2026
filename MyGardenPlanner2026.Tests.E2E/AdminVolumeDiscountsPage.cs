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

    /// <summary>Rækkens id (Guid) udledt af input-id'et "min-&lt;guid&gt;". Samme værdi som EntityId i AuditLog.</summary>
    public static async Task<Guid> ReadTierIdAsync(IPage page, int index)
    {
        var id = await MinInput(page, index).GetAttributeAsync("id")
            ?? throw new InvalidOperationException($"Række {index} har ikke noget min-input med id.");

        return Guid.Parse(id["min-".Length..]);
    }

    /// <summary>
    /// Opretter et trin via "Tilføj trappe" (Max tom = åben trappe) med step-up, hvis modalen vises.
    /// Et trin med højeste MinGardens havner som sidste række (indeks = antal rækker før oprettelsen).
    /// </summary>
    public static async Task AddTierAsync(IPage page, SmokeTestUser user, int minGardens, decimal multiplier)
    {
        var form = page.Locator(".volume-discount-editor .form-section");

        await SetValueAsync(form.Locator("#new-min"), minGardens.ToString(CultureInfo.InvariantCulture));
        await SetValueAsync(form.Locator("#new-mult"), multiplier.ToString("0.00", CultureInfo.InvariantCulture));

        await AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            form.Locator("#add-tier"),
            page.Locator(".status-success")
                .Filter(new() { HasText = $"Ny volumenrabat-trappe fra {minGardens} haver er oprettet." }),
            Error(page),
            $"Oprettelse af volumenrabat-trappen fra {minGardens} haver");
    }

    /// <summary>Sletter rækken ("Slet") med step-up, hvis modalen vises.</summary>
    public static Task DeleteTierAsync(IPage page, SmokeTestUser user, int index) =>
        AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            Row(page, index).GetByRole(AriaRole.Button, new() { Name = "Slet", Exact = true }),
            page.Locator(".status-success").Filter(new() { HasText = "Volumenrabat-trappen er slettet." }),
            Error(page),
            $"Sletning af volumenrabat-trin på række {index}");

    private static async Task SetValueAsync(ILocator input, string value)
    {
        await input.FillAsync(value);
        await input.BlurAsync(); // @onchange udløses først ved blur
    }
}
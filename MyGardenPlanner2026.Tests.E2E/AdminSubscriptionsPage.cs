namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

/// <summary>
/// Page-object for /admin/subscriptions, fanen "Basispriser" (BasePriceMatrixEditor).
/// Gem er pr. række og udløser step-up. Et vellykket gem genindlæser alle rækker og
/// nulstiller ugemte indtastninger, så rækker redigeres og gemmes én ad gangen.
/// </summary>
public static class AdminSubscriptionsPage
{
    public const string Title = "Administrer abonnementer – MyGardenPlanner";
    public const string Heading = "Administrer abonnementer";

    public const string BasePricesTab = "Basispriser";
    public const string VolumeDiscountsTab = "Volumen-rabatter";
    public const string AddOnsTab = "Tilkøbsmoduler";

    /// <summary>3 lag × 4 adgangskategorier (seed, Prismatrix tabel 1).</summary>
    public const int SeededTierCount = 12;

    /// <summary>Logger ind, åbner siden og venter, til den er interaktiv. Slutter på fanen "Basispriser".</summary>
    public static async Task<IPage> OpenAsync(PlaywrightAppFixture fixture, SmokeTestUser user, int? width = null)
    {
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminSubscriptions, width);

        await SmokePageAssertions.ExpectPageAsync(page, Title, Heading);
        await WaitUntilInteractiveAsync(page);

        return page;
    }

    /// <summary>
    /// Fanehop væk og tilbage beviser et forbundet kredsløb og genopretter editoren.
    /// Rækkerne vises først, når priserne er indlæst.
    /// </summary>
    public static async Task WaitUntilInteractiveAsync(IPage page)
    {
        await TabNavigation.SelectAsync(page, VolumeDiscountsTab);
        await TabNavigation.SelectAsync(page, BasePricesTab);
        await Assertions.Expect(TierRows(page)).ToHaveCountAsync(SeededTierCount);
    }

    public static ILocator TierRows(IPage page) => page.Locator(".base-price-matrix-editor tbody tr");

    /// <summary>Samme navn som seederen og editorens labels: "&lt;lag&gt; · &lt;kategori&gt;".</summary>
    public static string TierName(GardenAccessLevel level, AccessCategory category) =>
        $"{level.ToDisplayName()} · {category}";

    public static ILocator PriceInput(
        IPage page, BillingCycle cycle, GardenAccessLevel level, AccessCategory category) =>
        page.GetByLabel($"{LabelPrefix(cycle)} {TierName(level, category)}", new() { Exact = true });

    public static ILocator SaveButton(IPage page, GardenAccessLevel level, AccessCategory category) =>
        TierRows(page)
            .Filter(new() { Has = PriceInput(page, BillingCycle.Annual, level, category) })
            .GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true });

    /// <summary>Succesbeskeden for en gemt række (ryddes ved fanehop).</summary>
    public static ILocator SavedMessage(IPage page, GardenAccessLevel level, AccessCategory category) =>
        page.Locator(".status-success")
            .Filter(new() { HasText = $"Basispris for '{TierName(level, category)}' er opdateret." });

    public static ILocator Error(IPage page) => page.Locator(".base-price-matrix-editor .status-danger");

    public static async Task<decimal> ReadPriceAsync(
        IPage page, BillingCycle cycle, GardenAccessLevel level, AccessCategory category) =>
        decimal.Parse(
            await PriceInput(page, cycle, level, category).InputValueAsync(),
            CultureInfo.InvariantCulture);

    public static async Task SetPriceAsync(
        IPage page, BillingCycle cycle, GardenAccessLevel level, AccessCategory category, decimal value)
    {
        var input = PriceInput(page, cycle, level, category);
        await input.FillAsync(value.ToString("0.00", CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @onchange udløses først ved blur
    }

    /// <summary>
    /// Klikker "Gem" på rækken og gennemfører step-up, hvis modalen vises (se <see cref="AdminSaveFlow"/>).
    /// Må ikke kaldes to gange for samme række i samme side (beskeden består).
    /// </summary>
    public static Task SaveTierAsync(
        IPage page, SmokeTestUser user, GardenAccessLevel level, AccessCategory category) =>
        AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            SaveButton(page, level, category),
            SavedMessage(page, level, category),
            Error(page),
            $"Gem af '{TierName(level, category)}'");

    private static string LabelPrefix(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Annual => "Årlig pris for",
        BillingCycle.Monthly => "Månedlig pris for",
        BillingCycle.Perpetual => "Perpetual-pris for",
        _ => throw new ArgumentOutOfRangeException(nameof(cycle), cycle, "Ukendt betalingscyklus."),
    };
}
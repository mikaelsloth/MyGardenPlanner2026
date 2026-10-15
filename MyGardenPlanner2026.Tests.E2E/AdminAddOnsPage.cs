namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using System.Globalization;

/// <summary>
/// Page-object for /admin/subscriptions, fanen "Tilkøbsmoduler" (AddOnEditor). Rækkerne har ens labels
/// og findes via typen (første kolonne). Gem er pr. række og udløser step-up; et vellykket gem
/// genindlæser alle rækker og nulstiller ugemte indtastninger.
/// </summary>
public static class AdminAddOnsPage
{
    /// <summary>5 tilkøb i seed (Prismatrix tabel 3).</summary>
    public const int SeededAddOnCount = 5;

    /// <summary>Logger ind, åbner siden og fanen og venter på, at rækkerne er indlæst (antal kontrolleres ikke).</summary>
    public static async Task<IPage> OpenAsync(PlaywrightAppFixture fixture, SmokeTestUser user, int? width = null)
    {
        var page = await AdminSubscriptionsPage.OpenAsync(fixture, user, width);

        await TabNavigation.SelectAsync(page, AdminSubscriptionsPage.AddOnsTab);
        await Assertions.Expect(Rows(page).First).ToBeVisibleAsync();

        return page;
    }

    public static ILocator Rows(IPage page) => page.Locator(".add-on-editor tbody tr");

    public static ILocator Row(IPage page, AddOnType type) =>
        Rows(page).Filter(new() { Has = page.GetByText(type.ToString(), new() { Exact = true }) });

    public static ILocator NameInput(IPage page, AddOnType type) => Row(page, type).Locator("input[id^='name-']");

    public static ILocator PriceInput(IPage page, AddOnType type, BillingCycle cycle) =>
        Row(page, type).Locator($"input[id^='{IdPrefix(cycle)}-']");

    public static ILocator SaveButton(IPage page, AddOnType type) =>
        Row(page, type).GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true });

    /// <summary>Succesbesked for en gemt række (navnet er rækkens uændrede navn).</summary>
    public static ILocator SavedMessage(IPage page, string name) =>
        page.Locator(".status-success").Filter(new() { HasText = $"Tilkøb '{name}' er opdateret." });

    public static ILocator Error(IPage page) => page.Locator(".add-on-editor .status-danger");

    /// <summary>Fejlteksten, når typen allerede findes.</summary>
    public static string DuplicateTypeError(AddOnType type) =>
        $"Der findes allerede et tilkøb med typen '{type}'.";

    public static Task<string> ReadNameAsync(IPage page, AddOnType type) =>
        NameInput(page, type).InputValueAsync();

    public static async Task<decimal> ReadPriceAsync(IPage page, AddOnType type, BillingCycle cycle) =>
        decimal.Parse(await PriceInput(page, type, cycle).InputValueAsync(), CultureInfo.InvariantCulture);

    public static async Task SetPriceAsync(IPage page, AddOnType type, BillingCycle cycle, decimal value)
    {
        var input = PriceInput(page, type, cycle);
        await input.FillAsync(value.ToString("0.00", CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @onchange udløses først ved blur
    }

    public static Task SaveAddOnAsync(IPage page, SmokeTestUser user, AddOnType type, string name) =>
        AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            SaveButton(page, type),
            SavedMessage(page, name),
            Error(page),
            $"Gem af tilkøbet '{name}'");

    /// <summary>Forsøger at oprette et tilkøb med en type, der allerede findes, og forventer afvisning.</summary>
    public static async Task AddDuplicateAsync(IPage page, SmokeTestUser user, AddOnType type, string name)
    {
        var form = page.Locator(".add-on-editor .form-section");

        await form.Locator("#new-type").SelectOptionAsync(type.ToString());

        var nameInput = form.Locator("#new-name");
        await nameInput.FillAsync(name);
        await nameInput.BlurAsync(); // @bind på tekstfelt opdateres ved blur

        await AdminSaveFlow.RejectWithStepUpAsync(
            page,
            user,
            form.GetByRole(AriaRole.Button, new() { Name = "Tilføj tilkøb", Exact = true }),
            Error(page),
            DuplicateTypeError(type));
    }

    /// <summary>Sletter den sidste række (ekstra række oprettet ved en fejl). Må kun bruges én gang pr. side.</summary>
    public static Task DeleteLastRowAsync(IPage page, SmokeTestUser user) =>
        AdminSaveFlow.SaveWithStepUpAsync(
            page,
            user,
            Rows(page).Last.GetByRole(AriaRole.Button, new() { Name = "Slet", Exact = true }),
            page.Locator(".status-success").Filter(new() { HasText = "Tilkøbet er slettet." }),
            Error(page),
            "Sletning af ekstra tilkøb");

    private static string IdPrefix(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Annual => "annual",
        BillingCycle.Monthly => "monthly",
        BillingCycle.Perpetual => "perpetual",
        _ => throw new ArgumentOutOfRangeException(nameof(cycle), cycle, "Ukendt betalingscyklus."),
    };
}
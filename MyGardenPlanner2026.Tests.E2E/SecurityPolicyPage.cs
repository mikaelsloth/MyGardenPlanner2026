namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using System.Globalization;

/// <summary>Page-object for /admin/security-policies (fanerne og fanen AuditLog-eksport).</summary>
public static class SecurityPolicyPage
{
    public const string ExportJobSavedText = "AuditLog-eksportjob-policyen er opdateret og trådt i kraft.";
    public const string JitSavedText = "JIT-eskaleringspolicyen er opdateret og trådt i kraft.";
    public const string JitTabName = "JIT-eskalering";
    private const string OtherTabName = "Login rate limit";

    public static ILocator Retention(IPage page) => page.Locator("#exportjob-retention");

    public static ILocator SaveButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true });

    /// <summary>Statusbeskeden på siden (ryddes ved fanskift).</summary>
    public static ILocator Success(IPage page) => page.Locator(".status-success");

    // --- Faner (Tjekliste G) ---

    /// <summary>Fane på /admin/security-policies. Component indgår i skærmbilledenavne (ingen mellemrum).</summary>
    public sealed record PolicyTab(string Name, string Component, string SavedText, string PolicyName);

    public static readonly PolicyTab Jit = new(
        JitTabName, "Jit", JitSavedText, "JitElevationPolicy");

    public static readonly PolicyTab ReAuth = new(
        "Step-up re-auth", "ReAuth",
        "Step-up re-auth-policyen er opdateret og trådt i kraft.", "ReAuthenticationPolicy");

    public static readonly PolicyTab FailureTracker = new(
        "Fejl-tracker", "FejlTracker",
        "Fejl-tracker-policyen er opdateret og trådt i kraft.", "ReAuthFailureTrackerPolicy");

    public static readonly PolicyTab AdminApiRateLimit = new(
        "Admin-API rate limit", "AdminApi",
        "Admin-API rate limit-policyen er opdateret og trådt i kraft.", "AdminApiRateLimitPolicy");

    public static readonly PolicyTab LoginRateLimit = new(
        OtherTabName, "Login",
        "Login rate limit-policyen er opdateret og trådt i kraft.", "LoginRateLimitPolicy");

    public static readonly PolicyTab ExportJob = new(
        "AuditLog-eksport", "AuditLogEksport", ExportJobSavedText, "AuditLogExportJobPolicy");

    /// <summary>Alle faner i sidens rækkefølge.</summary>
    public static IReadOnlyList<PolicyTab> AllTabs { get; } =
        [Jit, ReAuth, FailureTracker, AdminApiRateLimit, LoginRateLimit, ExportJob];

    /// <summary>Id'er på policyfelterne (input) pr. fane.</summary>
    public static class FieldIds
    {
        public const string JitMin = "jit-min";
        public const string JitMax = "jit-max";
        public const string JitSweep = "jit-sweep";
        public const string ReAuthMaxAge = "reauth-maxage";
        public const string FailureThreshold = "failtrack-threshold";
        public const string FailureWindowDays = "failtrack-windowdays";
        public const string AdminApiPermit = "adminapi-permit";
        public const string AdminApiWindow = "adminapi-window";
        public const string AdminApiSegments = "adminapi-segments";
        public const string LoginPermit = "login-permit";
        public const string LoginWindow = "login-window";
        public const string ExportRetention = "exportjob-retention";
        public const string ExportMaxActive = "exportjob-maxactive";
    }

    public static ILocator Field(IPage page, string id) => page.Locator($"#{id}");

    /// <summary>Fejlbesked fra en policy-editor (role=alert).</summary>
    public static ILocator Error(IPage page) => page.Locator(".security-policy-editor .status-danger");

    private const string PageTitle = "Sikkerhedspolicies – MyGardenPlanner";
    private const string PageHeading = "Sikkerhedspolicies";

    /// <summary>Logger ind som brugeren og åbner siden og fanen i en frisk circuit.</summary>
    public static async Task<IPage> OpenAsync(
        PlaywrightAppFixture fixture, SmokeTestUser user, PolicyTab tab, string readyFieldId, int? width = null)
    {
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminSecurityPolicies, width);

        await SmokePageAssertions.ExpectPageAsync(page, PageTitle, PageHeading);
        await OpenTabAsync(page, tab, readyFieldId);

        return page;
    }

    /// <summary>
    /// Ny side i samme browser-context: allerede logget ind (ingen login, så login-limiten røres ikke),
    /// men en ny circuit uden re-auth-tidsstempel, så step-up modalen vises igen.
    /// </summary>
    public static async Task<IPage> OpenInNewCircuitAsync(
        PlaywrightAppFixture fixture, IPage existing, PolicyTab tab, string readyFieldId)
    {
        var page = await existing.Context.NewPageAsync();
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.AdminSecurityPolicies}");

        await SmokePageAssertions.ExpectPageAsync(page, PageTitle, PageHeading);
        await OpenTabAsync(page, tab, readyFieldId);

        return page;
    }

    /// <summary>Fanehop til en anden fane og tilbage beviser et forbundet kredsløb; derefter afventes feltet.</summary>
    public static async Task OpenTabAsync(IPage page, PolicyTab tab, string readyFieldId)
    {
        var hopTab = tab == LoginRateLimit ? Jit : LoginRateLimit;

        await SelectTabAsync(page, hopTab.Name);
        await SelectTabAsync(page, tab.Name);
        await Assertions.Expect(Field(page, readyFieldId)).ToBeVisibleAsync();
    }

    public static Task ExpectFieldValueAsync(IPage page, string id, int expected) =>
        Assertions.Expect(Field(page, id)).ToHaveValueAsync(expected.ToString(CultureInfo.InvariantCulture));

    public static Task SetFieldAsync(IPage page, string id, int value) =>
        SetNumberAsync(Field(page, id), value);

    /// <summary>
    /// Sætter ét felt, klikker "Gem" og gennemfører step-up. Kræver en frisk circuit
    /// (modalen skal vises). Slutter på fanens succesbesked.
    /// </summary>
    public static async Task SaveWithStepUpAsync(
        IPage page, SmokeTestUser user, string fieldId, int value, string savedText)
    {
        await SetFieldAsync(page, fieldId, value);

        await StepUpModal.OpenAsync(page, () => SaveButton(page).ClickAsync());
        await StepUpModal.CompleteAsync(page, user);

        await Assertions.Expect(Success(page)).ToContainTextAsync(savedText);
    }

    public static Task<int> ReadFieldAsync(IPage page, string id) =>
    ReadNumberAsync(Field(page, id));

    /// <summary>
    /// Åbner en ny circuit (ny step-up) og skriver kun værdien tilbage, hvis den afviger fra originalen,
    /// dvs. hvis testen nåede at ændre den og ikke selv gendannede. Kontrollerer altid slutværdien.
    /// </summary>
    public static async Task RestoreFieldAsync(
        PlaywrightAppFixture fixture, IPage previous, PolicyTab tab, string fieldId,
        SmokeTestUser user, int original)
    {
        var page = await OpenInNewCircuitAsync(fixture, previous, tab, fieldId);

        if (await ReadFieldAsync(page, fieldId) != original)
        {
            await SaveWithStepUpAsync(page, user, fieldId, original, tab.SavedText);
        }

        await ExpectFieldValueAsync(page, fieldId, original);
    }

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

    // --- Fanen JIT-eskalering (standardfane) ---

    public sealed record JitPolicyValues(int Min, int Max, int Sweep);

    public static ILocator JitMin(IPage page) => page.Locator("#jit-min");

    public static ILocator JitMax(IPage page) => page.Locator("#jit-max");

    public static ILocator JitSweep(IPage page) => page.Locator("#jit-sweep");

    /// <summary>
    /// JIT-eskalering er standardfanen ved indlæsning, men prerenderede indtastninger kan nulstilles.
    /// Fanehop til en anden fane og tilbage beviser et forbundet kredsløb; derefter genindlæser editoren.
    /// Felterne vises først, når policyen er indlæst (skeleton før).
    /// </summary>
    public static async Task OpenJitTabAsync(IPage page)
    {
        await SelectTabAsync(page, OtherTabName);
        await SelectTabAsync(page, JitTabName);
        await Assertions.Expect(JitMin(page)).ToBeVisibleAsync();
    }

    public static async Task<JitPolicyValues> ReadJitPolicyAsync(IPage page) => new(
        await ReadNumberAsync(JitMin(page)),
        await ReadNumberAsync(JitMax(page)),
        await ReadNumberAsync(JitSweep(page)));

    /// <summary>Sætter Min og Sweep (Max uændret), klikker "Gem" og gennemfører step-up. Slutter på succesbeskeden.</summary>
    public static async Task SaveJitPolicyAsync(IPage page, SmokeTestUser user, int min, int sweep)
    {
        await SetNumberAsync(JitMin(page), min);
        await SetNumberAsync(JitSweep(page), sweep);

        await StepUpModal.OpenAsync(page, () => SaveButton(page).ClickAsync());
        await StepUpModal.CompleteAsync(page, user);

        await Assertions.Expect(Success(page)).ToContainTextAsync(JitSavedText);
    }

    private static async Task<int> ReadNumberAsync(ILocator input) =>
        int.Parse(await input.InputValueAsync(), CultureInfo.InvariantCulture);

    private static async Task SetNumberAsync(ILocator input, int value)
    {
        await input.FillAsync(value.ToString(CultureInfo.InvariantCulture));
        await input.BlurAsync(); // @onchange udløses først ved blur
    }
}
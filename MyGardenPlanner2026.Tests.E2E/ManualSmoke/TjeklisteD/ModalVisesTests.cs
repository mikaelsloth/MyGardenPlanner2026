namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteD;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste D1: modal vises ved beskyttet handling. Login markerer kun re-auth i request-scope
/// (ReAuthenticationService er Scoped), så en frisk circuit har intet tidsstempel, og modalen
/// vises ved første handling uden ventetid. Modalen annulleres, så ingen data ændres.
/// Skærmbilleder: StepUp_&lt;Side&gt;_Side, StepUp_&lt;Side&gt;_Modal og StepUp_AccessDenied (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class ModalVisesTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste D";
    private const string TestName = "StepUp";

    private sealed record AdminPageSpec(
        string Component, string Persona, string Route, string Heading, Func<IPage, ILocator>? Trigger);

    private static ILocator FirstSaveButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true }).First;

    // Triggers er hentet fra Components-gennemgangen og er den eneste antagelse i testen.
    private static readonly AdminPageSpec[] Specs =
    [
        new("AdminSubscriptions", SmokeTestPersonas.Admin, E2ERoutes.AdminSubscriptions,
            "Administrer abonnementer", FirstSaveButton),
        new("AdminSecurityPolicies", SmokeTestPersonas.PolicyAdmin, E2ERoutes.AdminSecurityPolicies,
            "Sikkerhedspolicies", FirstSaveButton),
        new("AdminAuditLog", SmokeTestPersonas.Auditor, E2ERoutes.AdminAuditLog,
            "AuditLog", page => page.GetByRole(AriaRole.Button, new() { Name = "Eksportér" }).First),
        // Godkend/Afvis kræver en ventende anmodning fra en anden admin: kun siden dækkes her.
        new("AdminJitRequests", SmokeTestPersonas.DataAdmin, E2ERoutes.AdminJitRequests,
            "JIT-adgang", Trigger: null),
    ];

    public static TheoryData<string, int> ModalCases()
    {
        var data = new TheoryData<string, int>();
        foreach (var spec in Specs.Where(s => s.Trigger is not null))
        {
            foreach (var width in Viewports.All)
            {
                data.Add(spec.Component, width);
            }
        }

        return data;
    }

    public static TheoryData<int> Widths()
    {
        var data = new TheoryData<int>();
        foreach (var width in Viewports.All)
        {
            data.Add(width);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ModalCases))]
    public async Task AdminSide_VedBeskyttetHandling_ViserStepUpModalUdenVentetid(string component, int width)
    {
        var spec = Specs.Single(s => s.Component == component);
        var page = await fixture.LoginAndGotoAsync(spec.Persona, spec.Route, width);

        await ExpectAdminPageAsync(page, spec.Heading);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, $"{component}_Side");

        await StepUpModal.OpenAsync(page, () => spec.Trigger!(page).ClickAsync());

        await Assertions.Expect(StepUpModal.PasswordField(page)).ToBeVisibleAsync();
        // Admin-sider kræver 2FA, så TOTP-feltet vises altid.
        await Assertions.Expect(StepUpModal.TotpField(page)).ToBeVisibleAsync();
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, $"{component}_Modal");

        await StepUpModal.CancelButton(page).ClickAsync();
        await Assertions.Expect(StepUpModal.Dialog(page)).ToBeHiddenAsync();
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task JitSide_ForDataAdmin_LoaderOgGemmerSkaermbillede(int width)
    {
        var spec = Specs.Single(s => s.Component == "AdminJitRequests");
        var page = await fixture.LoginAndGotoAsync(spec.Persona, spec.Route, width);

        await ExpectAdminPageAsync(page, spec.Heading);
        await Assertions.Expect(StepUpModal.Dialog(page)).ToHaveCountAsync(0);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, $"{spec.Component}_Side");
    }

    [Theory]
    [InlineData(E2ERoutes.AdminAuditLog)]
    [InlineData(E2ERoutes.AdminSecurityPolicies)]
    [InlineData(E2ERoutes.AdminSubscriptions)]
    [InlineData(E2ERoutes.AdminJitRequests)]
    public async Task Requester_UdenAdminRolle_AfvisesPaaAlleAdminsider(string route)
    {
        var page = await fixture.LoginAndGotoAsync(SmokeTestPersonas.Requester, route);

        page.Url.Should().Contain(E2ERoutes.AccessDenied);
        await SmokePageAssertions.ExpectPageAsync(page, "Adgang nægtet – MyGardenPlanner", "Adgang nægtet");
        await Assertions.Expect(page.GetByText("Du har ikke adgang til denne ressource.")).ToBeVisibleAsync();
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task Requester_AfvisesPaaAuditLog_GemmerAccessDeniedSkaermbillede(int width)
    {
        var page = await fixture.LoginAndGotoAsync(SmokeTestPersonas.Requester, E2ERoutes.AdminAuditLog, width);

        await SmokePageAssertions.ExpectPageAsync(page, "Adgang nægtet – MyGardenPlanner", "Adgang nægtet");
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AccessDenied");
    }

    private static async Task ExpectAdminPageAsync(IPage page, string heading)
    {
        var h1 = page.GetByRole(AriaRole.Heading, new() { Level = 1 });
        await Assertions.Expect(h1).ToHaveCountAsync(1);
        await Assertions.Expect(h1).ToContainTextAsync(heading);
        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    }
}
namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteI;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;
using AdminPage = MyGardenPlanner2026.Tests.E2E.AdminAccessMatrix.AdminPage;

/// <summary>
/// Tjekliste I1: AdminNavMenu viser kun links, brugeren reelt har adgang til. Forventningerne kommer fra
/// <see cref="AdminAccessMatrix"/>. I1a-c: #2 DataAdmin, #3 PolicyAdmin og #4 Auditor hver for sig (+ #1 Admin som
/// kontrol), i 768 og 1280. Hvert synligt link åbner en side, brugeren kan tilgå. I1d: en frisk AuditViewer får
/// godkendt JIT SystemAdmin, og navigationen viser derefter også "Abonnementer" (efter ny sideindlæsning, da
/// navigationen er statisk). Skærmbilleder: Adgangskontrol_I1_&lt;Persona&gt;_&lt;bredde&gt; og
/// Adgangskontrol_I1d_&lt;bredde&gt;.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class AdminNavigationTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste I";
    private const string TestName = "Adgangskontrol";

    private const int Minutes = 30;

    [GeneratedRegex(@"(^|\s)active(\s|$)")]
    private static partial Regex ActiveClass { get; }

    public static TheoryData<string, int> Cases()
    {
        var data = new TheoryData<string, int>();
        foreach (var persona in AdminAccessMatrix.Personas)
        {
            data.Add(persona, Viewports.Tablet);
            data.Add(persona, Viewports.Desktop);
        }

        return data;
    }

    public static TheoryData<string> Personas()
    {
        var data = new TheoryData<string>();
        foreach (var persona in AdminAccessMatrix.Personas)
        {
            data.Add(persona);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Navigation_ViserKunLinksPersonaenHarAdgangTil_OgGemmerSkaermbillede(string persona, int width)
    {
        var access = AdminAccessMatrix.For(persona);
        var page = await fixture.LoginAndGotoAsync(persona, access.Start.Route, width);

        await SmokePageAssertions.ExpectPageAsync(page, access.Start.Title, access.Start.Heading);
        await AdminNavigation.ExpectExactLinksAsync(page, access.Allowed);
        await Assertions.Expect(AdminNavigation.Link(page, access.Start)).ToHaveClassAsync(ActiveClass);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, $"I1_{persona}");
    }

    [Theory]
    [MemberData(nameof(Personas))]
    public async Task Navigation_HvertSynligtLink_AabnerSideBrugerenKanTilgaa(string persona)
    {
        var access = AdminAccessMatrix.For(persona);
        var page = await fixture.LoginAndGotoAsync(persona, access.Start.Route, Viewports.Desktop);

        await SmokePageAssertions.ExpectPageAsync(page, access.Start.Title, access.Start.Heading);

        foreach (var target in access.Allowed.Where(p => p != access.Start))
        {
            await AdminNavigation.Link(page, target).ClickAsync();

            // Et afvist link ville ende på /Account/AccessDenied i stedet for målet.
            await Assertions.Expect(page).ToHaveURLAsync(UrlOf(target));
            await SmokePageAssertions.ExpectPageAsync(page, target.Title, target.Heading);
            await AdminNavigation.ExpectExactLinksAsync(page, access.Allowed);
        }
    }

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Navigation_EfterGodkendtJitSystemAdmin_ViserOgsaaAbonnementer(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var admin = fixture.SmokeTestUsers[SmokeTestPersonas.Admin];
        var reason = JitRequestsPage.NewReason();

        AdminAccessMatrix.AdminPage[] before = [AdminAccessMatrix.JitRequests, AdminAccessMatrix.AuditLog];
        AdminAccessMatrix.AdminPage[] after =
            [AdminAccessMatrix.Subscriptions, AdminAccessMatrix.JitRequests, AdminAccessMatrix.AuditLog];

        // Før: AuditViewer ser kun JIT-adgang og AuditLog.
        var pageA = await fixture.LoginAndGotoAsync(userA, AdminAccessMatrix.AuditLog.Route, width);
        await SmokePageAssertions.ExpectPageAsync(
            pageA, AdminAccessMatrix.AuditLog.Title, AdminAccessMatrix.AuditLog.Heading);
        await AdminNavigation.ExpectExactLinksAsync(pageA, before);

        // A anmoder om SystemAdmin, Admin godkender (step-up).
        await JitRequestsPage.GotoAsync(fixture, pageA);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, reason);

        var pageAdmin = await JitRequestsPage.OpenAsync(fixture, admin);
        await JitRequestsPage.ApproveAsync(pageAdmin, admin, reason, RoleNames.SystemAdmin);

        // Efter: ny sideindlæsning (navigationen er statisk).
        await pageA.GotoAsync($"{fixture.RootUri}{AdminAccessMatrix.Subscriptions.Route}");
        await SmokePageAssertions.ExpectPageAsync(
            pageA, AdminAccessMatrix.Subscriptions.Title, AdminAccessMatrix.Subscriptions.Heading);
        await AdminNavigation.ExpectExactLinksAsync(pageA, after);

        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "I1d");
    }

    private static Regex UrlOf(AdminPage target) =>
        new($"{Regex.Escape(target.Route)}/?$", RegexOptions.IgnoreCase);
}
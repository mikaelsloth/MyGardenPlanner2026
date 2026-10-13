namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E3: godkendelse giver straks adgang. Ny AuditViewer (A) får AccessDenied på
/// /admin/subscriptions (kræver SystemAdmin), anmoder om SystemAdmin, og Admin (#1) godkender med
/// step-up. A åbner siden igen uden nyt login: JitRoleAuthorizationHandler læser DB ved hvert kald,
/// så adgangen virker ved næste sideindlæsning. Skærmbilleder: JIT_E3_Foer_&lt;bredde&gt; (AccessDenied)
/// og JIT_E3_Efter_&lt;bredde&gt; (siden).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class GodkendelseTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int Minutes = 30;
    private const string SubscriptionsHeading = "Administrer abonnementer";

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Godkendelse_GiverStraksAdgangTilBeskyttetSide(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var admin = fixture.SmokeTestUsers[SmokeTestPersonas.Admin];
        var reason = JitRequestsPage.NewReason();

        // Før: AuditViewer har ikke SystemAdmin.
        var pageA = await fixture.LoginAndGotoAsync(userA, E2ERoutes.AdminSubscriptions, width);
        await AccessAssertions.ExpectAccessDeniedAsync(pageA);
        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "E3_Foer");

        // A anmoder om SystemAdmin.
        await JitRequestsPage.GotoAsync(fixture, pageA);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, reason);

        // Admin godkender (step-up).
        var pageAdmin = await JitRequestsPage.OpenAsync(fixture, admin);
        await JitRequestsPage.ApproveAsync(pageAdmin, admin, reason, RoleNames.SystemAdmin);

        // Efter: A åbner siden igen uden nyt login.
        await pageA.GotoAsync($"{fixture.RootUri}{E2ERoutes.AdminSubscriptions}");
        await ExpectSubscriptionsPageAsync(pageA);
        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "E3_Efter");

        // A's række står som Godkendt (Godkendt vises uden "Vis afviste/udløbne").
        await JitRequestsPage.GotoAsync(fixture, pageA);
        await Assertions.Expect(JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(pageA, reason)))
            .ToHaveTextAsync(JitRequestsPage.ApprovedLabel);
    }

    private static async Task ExpectSubscriptionsPageAsync(IPage page)
    {
        var h1 = page.GetByRole(AriaRole.Heading, new() { Level = 1 });
        await Assertions.Expect(h1).ToHaveCountAsync(1);
        await Assertions.Expect(h1).ToContainTextAsync(SubscriptionsHeading);
        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);
        await Assertions.Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
    }
}
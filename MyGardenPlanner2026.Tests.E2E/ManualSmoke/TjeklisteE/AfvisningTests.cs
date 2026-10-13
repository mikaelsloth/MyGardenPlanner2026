namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E4: afvisning nægter adgang. Ny AuditViewer (A) anmoder to gange om SystemAdmin; Admin (#1)
/// afviser den ene med step-up. A får AccessDenied på /admin/subscriptions. Den afviste række er skjult
/// som standard (kun Afventer/Godkendt vises) og vises som "Afvist" med "Vis afviste/udløbne". Den anden,
/// stadig afventende række er positiv kontrol: den beviser, at listen er indlæst, før fraværet af den
/// afviste række asserteres. Skærmbilleder: JIT_E4_AccessDenied_&lt;bredde&gt;.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AfvisningTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int Minutes = 30;

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Afvisning_NaegterAdgangOgViserRaekkenSomAfvist(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var admin = fixture.SmokeTestUsers[SmokeTestPersonas.Admin];
        var rejectedReason = JitRequestsPage.NewReason();
        var controlReason = JitRequestsPage.NewReason();

        // A anmoder to gange om SystemAdmin.
        var pageA = await JitRequestsPage.OpenAsync(fixture, userA, width);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, rejectedReason);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, controlReason);

        // Admin afviser den ene (step-up).
        var pageAdmin = await JitRequestsPage.OpenAsync(fixture, admin);
        await JitRequestsPage.RejectAsync(pageAdmin, admin, rejectedReason, RoleNames.SystemAdmin);

        // Adgang nægtes.
        await pageA.GotoAsync($"{fixture.RootUri}{E2ERoutes.AdminSubscriptions}");
        await AccessAssertions.ExpectAccessDeniedAsync(pageA);
        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "E4_AccessDenied");

        // Den afviste række er skjult som standard (kontrolrækken beviser, at listen er indlæst) ...
        await JitRequestsPage.GotoAsync(fixture, pageA);
        var controlRow = JitRequestsPage.RequestRow(pageA, controlReason);
        await Assertions.Expect(controlRow).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.StatusBadge(controlRow)).ToHaveTextAsync(JitRequestsPage.PendingLabel);
        await Assertions.Expect(JitRequestsPage.RequestRow(pageA, rejectedReason)).ToHaveCountAsync(0);

        // ... og vises som Afvist med "Vis afviste/udløbne".
        await JitRequestsPage.ShowHistoryAsync(pageA);
        var rejectedRow = JitRequestsPage.RequestRow(pageA, rejectedReason);
        await Assertions.Expect(rejectedRow).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.StatusBadge(rejectedRow)).ToHaveTextAsync(JitRequestsPage.RejectedLabel);
    }
}
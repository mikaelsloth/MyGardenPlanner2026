namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E8: ansøger kan trække egen afventende anmodning tilbage. Ny AuditViewer (A) anmoder om SystemAdmin
/// og trækker den tilbage med bekræftelse: besked "er trukket tilbage", rækken er skjult som standard og vises som
/// "Annulleret" med "Vis afviste/udløbne/annullerede", uden "Træk tilbage"-knap. En anden ansøger (B) har en
/// kontrolrække om samme rolle: Admin (#1) ser den i sin kø, men ikke A's. A kan derefter anmode igen
/// (den tilbagetrukne tæller ikke som åben). Skærmbilleder: JIT_E8_Annulleret_&lt;bredde&gt;.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class TilbagetraekningTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int Minutes = 30;

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Ansoeger_KanTraekkeEgenAfventendeAnmodningTilbage(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var userB = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var reasonA = JitRequestsPage.NewReason();
        var controlReason = JitRequestsPage.NewReason();
        var newReason = JitRequestsPage.NewReason();

        // B's kontrolrække (afventende, forbliver i Admins kø).
        var pageB = await JitRequestsPage.OpenAsync(fixture, userB);
        await JitRequestsPage.RequestAndWaitAsync(pageB, RoleNames.SystemAdmin, Minutes, controlReason);

        // A anmoder og trækker tilbage.
        var pageA = await JitRequestsPage.OpenAsync(fixture, userA, width);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, reasonA);
        await JitRequestsPage.WithdrawAsync(pageA, reasonA, RoleNames.SystemAdmin);

        // Skjult som standard, vist som Annulleret med historik, uden knap.
        await Assertions.Expect(JitRequestsPage.RequestRow(pageA, reasonA)).ToHaveCountAsync(0);
        await JitRequestsPage.ShowHistoryAsync(pageA);
        var row = JitRequestsPage.RequestRow(pageA, reasonA);
        await Assertions.Expect(row).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.StatusBadge(row)).ToHaveTextAsync(JitRequestsPage.CancelledLabel);
        await Assertions.Expect(JitRequestsPage.WithdrawButton(pageA, reasonA)).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "E8_Annulleret");

        // Admins kø: kontrolrækken (positiv kontrol), ikke A's.
        var pageAdmin = await JitRequestsPage.OpenAsync(fixture, SmokeTestPersonas.Admin);
        await TabNavigation.SelectAsync(pageAdmin, JitRequestsPage.ApproveTabName);
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAdmin, controlReason)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAdmin, reasonA)).ToHaveCountAsync(0);

        // A kan anmode igen.
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, newReason);
        await Assertions.Expect(JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(pageA, newReason)))
            .ToHaveTextAsync(JitRequestsPage.PendingLabel);
    }
}
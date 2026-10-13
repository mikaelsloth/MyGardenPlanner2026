namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E2: ansøger ser ikke egen anmodning i godkendelseskøen. To nye AuditViewers (A og B)
/// anmoder hver om AuditViewer. A's kø viser B's række (positiv kontrol, så fraværet af A's egen er
/// meningsfuldt, da køen først renderer tom og derefter data), men ikke A's egen, som i stedet står
/// under "Mine anmodninger". Auditor (#4, direkte AuditViewer) ser begge rækker.
/// Skærmbilleder: JIT_E2_Koe_&lt;bredde&gt; (A's kø).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class EgenAnmodningIKoeTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int Minutes = 30;

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
    [MemberData(nameof(Widths))]
    public async Task Ansoeger_SeerIkkeEgenAnmodningIKoen_MenPeerOgAuditorGoer(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var userB = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var reasonA = JitRequestsPage.NewReason();
        var reasonB = JitRequestsPage.NewReason();

        var pageB = await JitRequestsPage.OpenAsync(fixture, userB);
        await JitRequestsPage.RequestAndWaitAsync(pageB, RoleNames.AuditViewer, Minutes, reasonB);

        var pageA = await JitRequestsPage.OpenAsync(fixture, userA, width);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.AuditViewer, Minutes, reasonA);

        // A's kø: B's række (positiv kontrol), ikke A's egen.
        await JitRequestsPage.SelectTabAsync(pageA, JitRequestsPage.ApproveTabName);
        await Assertions.Expect(JitRequestsPage.QueueRow(pageA, reasonB)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.QueueRow(pageA, reasonA)).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "E2_Koe");

        // A's egen anmodning står under "Mine anmodninger".
        await JitRequestsPage.SelectTabAsync(pageA, JitRequestsPage.RequestTabName);
        var ownRow = JitRequestsPage.RequestRow(pageA, reasonA);
        await Assertions.Expect(ownRow).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.StatusBadge(ownRow)).ToHaveTextAsync(JitRequestsPage.PendingLabel);

        // Auditor (#4) kan godkende begge og ser dem derfor begge.
        var pageAuditor = await JitRequestsPage.OpenAsync(fixture, SmokeTestPersonas.Auditor);
        await JitRequestsPage.SelectTabAsync(pageAuditor, JitRequestsPage.ApproveTabName);
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAuditor, reasonA)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAuditor, reasonB)).ToBeVisibleAsync();
    }
}
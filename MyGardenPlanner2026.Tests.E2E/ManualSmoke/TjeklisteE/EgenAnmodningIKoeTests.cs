namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E2: køen viser kun andres anmodninger om roller, brugeren selv er direkte medlem af.
/// A (AuditViewer) anmoder om SystemAdmin, B (PolicyAdmin) anmoder om AuditViewer.
/// A's kø viser B's række (positiv kontrol, da køen først renderer tom og derefter data), men ikke A's egen,
/// som i stedet står under "Mine anmodninger". Auditor (#4, AuditViewer) ser B's række, men ikke A's.
/// Admin (#1, SystemAdmin) ser A's række, men ikke B's.
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
    public async Task Koeen_ViserKunAndresAnmodningerForRollerBrugerenHar(int width)
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var userB = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var reasonA = JitRequestsPage.NewReason();
        var reasonB = JitRequestsPage.NewReason();

        var pageB = await JitRequestsPage.OpenAsync(fixture, userB);
        await JitRequestsPage.RequestAndWaitAsync(pageB, RoleNames.AuditViewer, Minutes, reasonB);

        var pageA = await JitRequestsPage.OpenAsync(fixture, userA, width);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, reasonA);

        // A's kø: B's række (positiv kontrol), ikke A's egen.
        await TabNavigation.SelectAsync(pageA, JitRequestsPage.ApproveTabName);
        await Assertions.Expect(JitRequestsPage.QueueRow(pageA, reasonB)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.QueueRow(pageA, reasonB)).ToContainTextAsync("@test.dk");
        await Assertions.Expect(JitRequestsPage.QueueRow(pageA, reasonB)).Not.ToContainTextAsync("(Slettet)");
        await Assertions.Expect(JitRequestsPage.QueueRow(pageA, reasonA)).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(pageA, Chapter, Checklist, TestName, "E2_Koe");

        // A's egen anmodning står under "Mine anmodninger".
        await TabNavigation.SelectAsync(pageA, JitRequestsPage.RequestTabName);
        var ownRow = JitRequestsPage.RequestRow(pageA, reasonA);
        await Assertions.Expect(ownRow).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.StatusBadge(ownRow)).ToHaveTextAsync(JitRequestsPage.PendingLabel);

        // Auditor (#4, AuditViewer): B's række, ikke A's (SystemAdmin).
        var pageAuditor = await JitRequestsPage.OpenAsync(fixture, SmokeTestPersonas.Auditor);
        await TabNavigation.SelectAsync(pageAuditor, JitRequestsPage.ApproveTabName);
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAuditor, reasonB)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAuditor, reasonA)).ToHaveCountAsync(0);

        // Admin (#1, SystemAdmin): A's række, ikke B's (AuditViewer).
        var pageAdmin = await JitRequestsPage.OpenAsync(fixture, SmokeTestPersonas.Admin);
        await TabNavigation.SelectAsync(pageAdmin, JitRequestsPage.ApproveTabName);
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAdmin, reasonA)).ToBeVisibleAsync();
        await Assertions.Expect(JitRequestsPage.QueueRow(pageAdmin, reasonB)).ToHaveCountAsync(0);
    }
}
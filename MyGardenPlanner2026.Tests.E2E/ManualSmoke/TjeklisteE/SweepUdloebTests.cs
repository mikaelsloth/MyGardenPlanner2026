namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste E5: lavt sweep-interval markerer udløbne eskaleringer som Expired. Ny PolicyAdmin sætter
/// MinRequestedMinutes = 1 og SweepIntervalMinutes = 1 (standard 30 og 5; uden Min = 1 kan en godkendt
/// anmodning ikke udløbe inden for rimelig tid). Ny AuditViewer (A) anmoder om SystemAdmin i 1 minut,
/// Admin (#1) godkender. RoleElevationExpirySweepService sætter kun Approved med ValidToUtc &lt; nu til
/// Expired, så testen poller A's liste (med "Vis afviste/udløbne"), til rækken står som "Udløbet" —
/// senest ca. 60 sekunder efter udløb. Policyen gendannes i finally via en ny circuit (ny step-up),
/// så E1 (standardgrænser 30-90) ikke påvirkes. Skærmbilleder: JIT_E5_Udloebet_&lt;bredde&gt;.
/// Testen tager ca. 2-4 minutter (appen kører med rigtig tid).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class SweepUdloebTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste E";
    private const string TestName = "JIT";

    private const int LowValue = 1;
    private const string ExpiredLabel = "Udløbet";

    private static readonly TimeSpan ExpiryTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task LavtSweepInterval_MarkererUdloebnGodkendtAnmodningSomExpired()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var admin = fixture.SmokeTestUsers[SmokeTestPersonas.Admin];
        var reason = JitRequestsPage.NewReason();

        var original = await ReadPolicyAsync(policyAdmin);

        try
        {
            await SavePolicyAsync(policyAdmin, LowValue, LowValue);

            // A anmoder om SystemAdmin i 1 minut, Admin godkender.
            var pageA = await JitRequestsPage.OpenAsync(fixture, userA);
            await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, LowValue, reason);

            var pageAdmin = await JitRequestsPage.OpenAsync(fixture, admin);
            await JitRequestsPage.ApproveAsync(pageAdmin, admin, reason, RoleNames.SystemAdmin);

            // Lige efter godkendelsen er der et minut til udløb: rækken står som Godkendt.
            await JitRequestsPage.RefreshRequestsAsync(pageA);
            await Assertions.Expect(JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(pageA, reason)))
                .ToHaveTextAsync(JitRequestsPage.ApprovedLabel);

            // Vent på sweep: kun Approved med ValidToUtc < nu bliver Expired.
            string? lastStatus = null;
            var expired = await BlazorInteraction.WaitUntilAsync(async () =>
            {
                lastStatus = await ReadStatusAsync(pageA, reason);
                if (lastStatus == ExpiredLabel)
                {
                    return true;
                }

                await Task.Delay(PollInterval, TestContext.Current.CancellationToken);
                return false;
            }, ExpiryTimeout);

            expired.Should().BeTrue(
                $"rækken skulle blive '{ExpiredLabel}' inden for {ExpiryTimeout}, men status var '{lastStatus}'");

            // Skærmbilleder i de tre bredder (nye sider for A, så det ikke kræver tre ventetider).
            foreach (var width in new[] { Viewports.Mobile, Viewports.Tablet, Viewports.Desktop })
            {
                var page = await JitRequestsPage.OpenAsync(fixture, userA, width);
                await JitRequestsPage.ShowHistoryAsync(page);
                await Assertions.Expect(JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(page, reason)))
                    .ToHaveTextAsync(ExpiredLabel);

                await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "E5_Udloebet");
            }
        }
        finally
        {
            await RestorePolicyAsync(policyAdmin, original);
        }
    }

    /// <summary>Ny side = ny circuit = step-up modalen vises ved første handling (re-auth-vinduet er kun 1 minut).</summary>
    private async Task<IPage> OpenPolicyPageAsync(SmokeTestUser user)
    {
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminSecurityPolicies, viewportWidth: null);

        await SmokePageAssertions.ExpectPageAsync(
            page, "Sikkerhedspolicies – MyGardenPlanner", "Sikkerhedspolicies");
        await SecurityPolicyPage.OpenJitTabAsync(page);

        return page;
    }

    private async Task<SecurityPolicyPage.JitPolicyValues> ReadPolicyAsync(SmokeTestUser user)
    {
        var page = await OpenPolicyPageAsync(user);
        return await SecurityPolicyPage.ReadJitPolicyAsync(page);
    }

    private async Task SavePolicyAsync(SmokeTestUser user, int min, int sweep)
    {
        var page = await OpenPolicyPageAsync(user);
        await SecurityPolicyPage.SaveJitPolicyAsync(page, user, min, sweep);
    }

    private async Task RestorePolicyAsync(SmokeTestUser user, SecurityPolicyPage.JitPolicyValues original)
    {
        var page = await OpenPolicyPageAsync(user);
        await SecurityPolicyPage.SaveJitPolicyAsync(page, user, original.Min, original.Sweep);

        (await SecurityPolicyPage.ReadJitPolicyAsync(page)).Should().Be(original);
    }

    /// <summary>Status for rækken (med "Vis afviste/udløbne"), eller null, hvis den ikke kunne læses endnu.</summary>
    private static async Task<string?> ReadStatusAsync(IPage page, string reason)
    {
        try
        {
            await JitRequestsPage.RefreshRequestsAsync(page, showHistory: true);

            var text = await JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(page, reason))
                .InnerTextAsync(new() { Timeout = 3000 });

            return text.Trim();
        }
        catch (PlaywrightException)
        {
            return null; // listen er midlertidigt væk under genindlæsning
        }
    }
}
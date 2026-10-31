namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G2 (JIT-eskalering): MinRequestedMinutes 30 → 15. En frisk AuditViewer anmoder om SystemAdmin
/// i 20 minutter: før ændringen afvises det, efter Gem accepteres det i samme circuit (uden genstart).
/// Policyen gendannes altid i finally via en ny circuit (ny step-up).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class JitPolicyIKraftTests(PlaywrightAppFixture fixture)
{
    private const int OriginalMin = 30;
    private const int ChangedMin = 15;
    private const int RequestedMinutes = 20;

    private const string OutOfRangeText = "Antal minutter skal være mellem 30 og 90.";

    [Fact]
    public async Task JitMinimum_AendretTil15_AccepterStraksAnmodningPaa20Minutter()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var requester = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var policyAdminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, policyAdmin.Email);

        // Før: 20 minutter er under minimum (30) og afvises.
        var requesterPage = await JitRequestsPage.OpenAsync(fixture, requester);
        await JitRequestsPage.SubmitRequestAsync(
            requesterPage, RoleNames.SystemAdmin, RequestedMinutes, JitRequestsPage.NewReason());

        var error = JitRequestsPage.Error(requesterPage);
        await Assertions.Expect(error).ToContainTextAsync(OutOfRangeText);
        await Assertions.Expect(error).ToContainTextAsync($"{RequestedMinutes} minutter ligger uden for grænsen");

        var policyPage = await SecurityPolicyPage.OpenAsync(fixture, policyAdmin, SecurityPolicyPage.Jit, Ids.JitMin);
        await SecurityPolicyPage.ExpectFieldValueAsync(policyPage, Ids.JitMin, OriginalMin);

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                policyPage, policyAdmin, Ids.JitMin, ChangedMin, SecurityPolicyPage.Jit.SavedText);
            await SecurityAlerts.ExpectPolicyChangedAsync(
                fixture.SmtpServer, policyAdminId, SecurityPolicyPage.Jit.PolicyName);

            // Efter: samme side hos ansøgeren, uden genstart.
            var reason = JitRequestsPage.NewReason();
            await JitRequestsPage.RequestAndWaitAsync(requesterPage, RoleNames.SystemAdmin, RequestedMinutes, reason);

            await Assertions.Expect(JitRequestsPage.StatusBadge(JitRequestsPage.RequestRow(requesterPage, reason)))
                .ToHaveTextAsync(JitRequestsPage.PendingLabel);
            await Assertions.Expect(JitRequestsPage.Error(requesterPage)).ToHaveCountAsync(0);
        }
        finally
        {
            await RestoreAsync(policyPage, policyAdmin);
        }
    }

    /// <summary>Ny circuit (ny step-up): skriver seed-værdien tilbage og kontrollerer den.</summary>
    private async Task RestoreAsync(IPage previous, SmokeTestUser user)
    {
        var page = await SecurityPolicyPage.OpenInNewCircuitAsync(
            fixture, previous, SecurityPolicyPage.Jit, Ids.JitMin);

        await SecurityPolicyPage.SaveWithStepUpAsync(
            page, user, Ids.JitMin, OriginalMin, SecurityPolicyPage.Jit.SavedText);
        await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.JitMin, OriginalMin);
    }
}
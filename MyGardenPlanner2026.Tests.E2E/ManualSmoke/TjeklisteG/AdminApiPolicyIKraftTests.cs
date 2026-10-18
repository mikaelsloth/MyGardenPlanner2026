namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G2 (Admin-API rate limit): PermitLimit 100 → 10. Limiteren (AdminActionRateLimiter) er pr. bruger
/// og ikke HTTP: en frisk admin B sender 10 JIT-anmodninger (hver går gennem AdminActionGuard), og den 11.
/// afvises med UI-fejlen "For mange handlinger på kort tid". Ingen HTTP 429. Policyen gendannes i finally.
/// Hver afsendt anmodning efterlader en Afventer-række i engangsdatabasen (uden betydning for andre tests).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AdminApiPolicyIKraftTests(PlaywrightAppFixture fixture)
{
    private const int OriginalPermit = 100;
    private const int ChangedPermit = 10;
    private const int Minutes = 30;

    private const string RateLimitedText = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";

    [Fact]
    public async Task PermitLimit_AendretTil10_AfviserDen11HandlingFraEnAdmin()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var admin = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var policyAdminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, policyAdmin.Email);

        var policyPage = await SecurityPolicyPage.OpenAsync(
            fixture, policyAdmin, SecurityPolicyPage.AdminApiRateLimit, Ids.AdminApiPermit);
        await SecurityPolicyPage.ExpectFieldValueAsync(policyPage, Ids.AdminApiPermit, OriginalPermit);

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                policyPage, policyAdmin, Ids.AdminApiPermit, ChangedPermit,
                SecurityPolicyPage.AdminApiRateLimit.SavedText);
            await SecurityAlerts.ExpectPolicyChangedAsync(
                fixture.SmtpServer, policyAdminId, SecurityPolicyPage.AdminApiRateLimit.PolicyName);

            // B's kvote er ny (limiteren genopbygges ved ændringen). 10 handlinger lykkes ...
            var adminPage = await JitRequestsPage.OpenAsync(fixture, admin);

            for (var i = 0; i < ChangedPermit; i++)
            {
                await JitRequestsPage.RequestAndWaitAsync(
                    adminPage, RoleNames.SystemAdmin, Minutes, JitRequestsPage.NewReason());
            }

            await Assertions.Expect(JitRequestsPage.Error(adminPage)).ToHaveCountAsync(0);

            // ... og den 11. afvises.
            var blockedReason = JitRequestsPage.NewReason();
            await JitRequestsPage.SubmitRequestAsync(adminPage, RoleNames.SystemAdmin, Minutes, blockedReason);

            await Assertions.Expect(JitRequestsPage.Error(adminPage)).ToContainTextAsync(RateLimitedText);
            await Assertions.Expect(JitRequestsPage.RequestRow(adminPage, blockedReason)).ToHaveCountAsync(0);
        }
        finally
        {
            await SecurityPolicyPage.RestoreFieldAsync(
                fixture, policyPage, SecurityPolicyPage.AdminApiRateLimit, Ids.AdminApiPermit,
                policyAdmin, OriginalPermit);
        }
    }
}
namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteE;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Tjekliste E7: JIT sender sikkerhedsalarm til Smtp:AdminSecurityEmails ved anmodning og ved godkendelse.
/// Ny AuditViewer (A) anmoder om SystemAdmin: alarmen indeholder A's Bruger-ID, rolle og minutter. Admin (#1)
/// godkender med step-up: alarmen indeholder A's Bruger-ID, Admins Godkender-ID, rolle og minutter.
/// Mails genkendes på A's Bruger-ID (A er ny, så mailen er unik). Ingen skærmbilleder.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class SikkerhedsalarmTests(PlaywrightAppFixture fixture)
{
    private const int Minutes = 30;

    [Fact]
    public async Task AnmodningOgGodkendelse_SenderSikkerhedsalarmTilAdminlisten()
    {
        var userA = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var admin = fixture.SmokeTestUsers[SmokeTestPersonas.Admin];
        var userAId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, userA.Email);
        var adminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, admin.Email);
        var reason = JitRequestsPage.NewReason();

        var pageA = await JitRequestsPage.OpenAsync(fixture, userA);
        await JitRequestsPage.RequestAndWaitAsync(pageA, RoleNames.SystemAdmin, Minutes, reason);
        await SecurityAlerts.ExpectJitRequestedAsync(fixture.SmtpServer, userAId, RoleNames.SystemAdmin, Minutes);

        var pageAdmin = await JitRequestsPage.OpenAsync(fixture, admin);
        await JitRequestsPage.ApproveAsync(pageAdmin, admin, reason, RoleNames.SystemAdmin);
        await SecurityAlerts.ExpectJitApprovedAsync(
            fixture.SmtpServer, userAId, adminId, RoleNames.SystemAdmin, Minutes);
    }
}
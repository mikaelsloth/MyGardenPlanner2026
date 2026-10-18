namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G2 (AuditLog-eksport): RetentionHours 24 → 1. En frisk AuditViewer sender et baggrundseksport;
/// når jobbet er fuldført, er ExpiresAtUtc − CompletedAtUtc ca. 1 time (læst direkte fra databasen, da
/// udløbet ikke vises i UI). Policyen gendannes altid i finally via en ny circuit.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class ExportJobPolicyIKraftTests(PlaywrightAppFixture fixture)
{
    private const int OriginalRetention = 24;
    private const int ChangedRetention = 1;

    private static readonly TimeSpan JobTimeout = TimeSpan.FromSeconds(60);

    private sealed record JobTimes(DateTimeOffset CompletedAtUtc, DateTimeOffset ExpiresAtUtc);

    [Fact]
    public async Task Retention_AendretTil1_GiverNyeEksportjobEnTimesLevetid()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var auditor = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var policyAdminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, policyAdmin.Email);
        var auditorId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, auditor.Email);

        var policyPage = await SecurityPolicyPage.OpenAsync(
            fixture, policyAdmin, SecurityPolicyPage.ExportJob, Ids.ExportRetention);
        await SecurityPolicyPage.ExpectFieldValueAsync(policyPage, Ids.ExportRetention, OriginalRetention);

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                policyPage, policyAdmin, Ids.ExportRetention, ChangedRetention,
                SecurityPolicyPage.ExportJob.SavedText);
            await SecurityAlerts.ExpectPolicyChangedAsync(
                fixture.SmtpServer, policyAdminId, SecurityPolicyPage.ExportJob.PolicyName);

            await EnqueueBackgroundExportAsync(auditor);

            var times = await WaitForCompletedJobAsync(auditorId);

            (times.ExpiresAtUtc - times.CompletedAtUtc).Should().BeCloseTo(
                TimeSpan.FromHours(ChangedRetention), TimeSpan.FromMinutes(1),
                "jobbet er fuldført efter ændringen og skal bruge den nye retention");
        }
        finally
        {
            await RestoreAsync(policyPage, policyAdmin);
        }
    }

    private async Task EnqueueBackgroundExportAsync(SmokeTestUser auditor)
    {
        var page = await fixture.LoginAndGotoAsync(auditor, E2ERoutes.AdminAuditLog, viewportWidth: null);
        await SmokePageAssertions.ExpectPageAsync(page, "AuditLog – MyGardenPlanner", "AuditLog");

        var enqueueButton = page.GetByRole(AriaRole.Button, new() { Name = "Send til baggrundseksport", Exact = true });

        await StepUpModal.OpenAsync(page, () => enqueueButton.ClickAsync());
        await StepUpModal.CompleteAsync(page, auditor);

        await Assertions.Expect(page.Locator(".audit-log-export-panel .status-success"))
            .ToContainTextAsync("Eksporten er sat i kø.");
    }

    /// <summary>Poller databasen (baggrundstjenesten poller hvert 10. sek.), til jobbet er fuldført med udløbstidspunkt.</summary>
    private async Task<JobTimes> WaitForCompletedJobAsync(string userId)
    {
        await using var context = new PlannerDbContext(
            E2ETestDatabase.CreateContextOptions(fixture.AppConnectionString));

        JobTimes? times = null;

        var completed = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            times = await context.Set<AuditLogExportJob>()
                .AsNoTracking()
                .Where(j => j.RequestedByUserId == userId && j.CompletedAtUtc != null && j.ExpiresAtUtc != null)
                .Select(j => new JobTimes(j.CompletedAtUtc!.Value, j.ExpiresAtUtc!.Value))
                .SingleOrDefaultAsync(TestContext.Current.CancellationToken);

            return times is not null;
        }, JobTimeout);

        completed.Should().BeTrue($"eksportjobbet skulle være fuldført inden for {JobTimeout}");

        return times!;
    }

    private async Task RestoreAsync(IPage previous, SmokeTestUser user)
    {
        var page = await SecurityPolicyPage.OpenInNewCircuitAsync(
            fixture, previous, SecurityPolicyPage.ExportJob, Ids.ExportRetention);

        await SecurityPolicyPage.SaveWithStepUpAsync(
            page, user, Ids.ExportRetention, OriginalRetention, SecurityPolicyPage.ExportJob.SavedText);
        await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.ExportRetention, OriginalRetention);
    }
}
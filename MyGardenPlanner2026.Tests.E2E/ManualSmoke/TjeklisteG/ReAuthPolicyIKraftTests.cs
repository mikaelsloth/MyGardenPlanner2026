namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Diagnostics;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G2 (Step-up re-auth): MaxAgeMinutes 1 → 10 (E2E-seed er 1, appsettings 15). Efter Gem (step-up)
/// ventes, til det gamle vindue (1 min) ville være udløbet. En ny Gem (gendan 1) i samme circuit viser da
/// ingen modal, fordi vinduet nu er 10 min. En ny side (ny circuit) viser modalen igen. Testen tager
/// ca. 75 sekunder (appen kører med rigtig tid).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class ReAuthPolicyIKraftTests(PlaywrightAppFixture fixture)
{
    private const int OriginalMaxAge = PlaywrightAppFixture.ReAuthMaxAgeMinutes;
    private const int ChangedMaxAge = 10;

    private static readonly TimeSpan OriginalWindow = TimeSpan.FromMinutes(OriginalMaxAge);
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task MaxAge_AendretTil10_GiverIngenModalEfterDetGamleVinduesUdloeb()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var policyAdminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, policyAdmin.Email);

        var page = await SecurityPolicyPage.OpenAsync(
            fixture, policyAdmin, SecurityPolicyPage.ReAuth, Ids.ReAuthMaxAge);
        await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.ReAuthMaxAge, OriginalMaxAge);

        try
        {
            // Frisk circuit: step-up modalen vises, og ændringen gemmes.
            await SecurityPolicyPage.SaveWithStepUpAsync(
                page, policyAdmin, Ids.ReAuthMaxAge, ChangedMaxAge, SecurityPolicyPage.ReAuth.SavedText);
            var sinceStepUp = Stopwatch.StartNew();

            await SecurityAlerts.ExpectPolicyChangedAsync(
                fixture.SmtpServer, policyAdminId, SecurityPolicyPage.ReAuth.PolicyName);

            // Vent, til det gamle vindue (1 min) ville være udløbet.
            var remaining = OriginalWindow + ExpiryMargin - sinceStepUp.Elapsed;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, TestContext.Current.CancellationToken);
            }

            // Fanehop rydder succesbeskeden (så en ny besked beviser en ny gem) og genindlæser editoren.
            await SecurityPolicyPage.SelectTabAsync(page, SecurityPolicyPage.Jit.Name);
            await SecurityPolicyPage.SelectTabAsync(page, SecurityPolicyPage.ReAuth.Name);
            await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.ReAuthMaxAge, ChangedMaxAge);

            // Samme circuit, efter det gamle vindues udløb: ingen modal, fordi vinduet nu er 10 min.
            await SecurityPolicyPage.SetFieldAsync(page, Ids.ReAuthMaxAge, OriginalMaxAge);
            await SecurityPolicyPage.SaveButton(page).ClickAsync();

            await Assertions.Expect(SecurityPolicyPage.Success(page))
                .ToContainTextAsync(SecurityPolicyPage.ReAuth.SavedText);
            await Assertions.Expect(StepUpModal.Dialog(page)).ToHaveCountAsync(0);
            await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.ReAuthMaxAge, OriginalMaxAge);

            // Ny circuit: ny modal ved første handling.
            var freshPage = await SecurityPolicyPage.OpenInNewCircuitAsync(
                fixture, page, SecurityPolicyPage.ReAuth, Ids.ReAuthMaxAge);

            await StepUpModal.OpenAsync(freshPage, () => SecurityPolicyPage.SaveButton(freshPage).ClickAsync());
            await StepUpModal.CancelButton(freshPage).ClickAsync();
            await Assertions.Expect(StepUpModal.Dialog(freshPage)).ToBeHiddenAsync();
        }
        finally
        {
            await SecurityPolicyPage.RestoreFieldAsync(
                fixture, page, SecurityPolicyPage.ReAuth, Ids.ReAuthMaxAge, policyAdmin, OriginalMaxAge);
        }
    }
}
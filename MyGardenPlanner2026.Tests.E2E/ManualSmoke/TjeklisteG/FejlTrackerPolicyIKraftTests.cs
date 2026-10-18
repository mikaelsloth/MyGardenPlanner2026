namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G2 (Fejl-tracker): Threshold 5 → 2. En ny bruger uden 2FA taster to gange forkert password:
/// efter 1. forsøg sendes ingen alarm, efter 2. sendes sikkerhedsalarmen (kun ved præcis Threshold).
/// Policyen gendannes altid i finally via en ny circuit.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class FejlTrackerPolicyIKraftTests(PlaywrightAppFixture fixture)
{
    private const int OriginalThreshold = 5;
    private const int ChangedThreshold = 2;

    private const string WrongPassword = "Forkert-Adgangskode1!";

    // Login.razor.cs: "Error: Invalid login attempt." (engelsk, ikke oversat endnu).
    private const string InvalidLoginText = "Invalid login attempt";

    [Fact]
    public async Task Threshold_AendretTil2_SenderAlarmVedAndetForkerteForsoeg()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var policyAdminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, policyAdmin.Email);

        var policyPage = await SecurityPolicyPage.OpenAsync(
            fixture, policyAdmin, SecurityPolicyPage.FailureTracker, Ids.FailureThreshold);
        await SecurityPolicyPage.ExpectFieldValueAsync(policyPage, Ids.FailureThreshold, OriginalThreshold);

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                policyPage, policyAdmin, Ids.FailureThreshold, ChangedThreshold,
                SecurityPolicyPage.FailureTracker.SavedText);
            await SecurityAlerts.ExpectPolicyChangedAsync(
                fixture.SmtpServer, policyAdminId, SecurityPolicyPage.FailureTracker.PolicyName);

            var page = await fixture.NewPageAsync();
            var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);
            var userId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, user.Email);

            await WrongLoginAsync(page, user);
            fixture.SmtpServer.Messages.Should().NotContain(
                m => IsAlertFor(m, userId), "1. forkerte forsøg ligger under den nye tærskel (2)");

            await WrongLoginAsync(page, user);
            var alert = await fixture.SmtpServer.WaitForMessageAsync(m => IsAlertFor(m, userId));

            alert.Recipients.Should().Equal(PlaywrightAppFixture.SecurityAlertRecipient);
            alert.Body.Should().Contain($"Bruger-ID: {userId}");
        }
        finally
        {
            await RestoreAsync(policyPage, policyAdmin);
        }
    }

    private static bool IsAlertFor(Smtp.ReceivedMail mail, string userId) =>
        mail.Subject == PlaywrightAppFixture.SecurityAlertSubject
        && mail.Body.Contains(userId, StringComparison.Ordinal);

    /// <summary>Frisk side pr. forsøg, så fejlteksten fra forrige forsøg ikke forveksles med det nye svar.</summary>
    private async Task WrongLoginAsync(IPage page, SmokeTestUser user)
    {
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.FillCredentialsAsync(page, user.Email, WrongPassword);
        await LoginFlow.LoginButton(page).ClickAsync();

        await Assertions.Expect(page.GetByText(InvalidLoginText)).ToBeVisibleAsync();
    }

    private async Task RestoreAsync(IPage previous, SmokeTestUser user)
    {
        var page = await SecurityPolicyPage.OpenInNewCircuitAsync(
            fixture, previous, SecurityPolicyPage.FailureTracker, Ids.FailureThreshold);

        await SecurityPolicyPage.SaveWithStepUpAsync(
            page, user, Ids.FailureThreshold, OriginalThreshold, SecurityPolicyPage.FailureTracker.SavedText);
        await SecurityPolicyPage.ExpectFieldValueAsync(page, Ids.FailureThreshold, OriginalThreshold);
    }
}
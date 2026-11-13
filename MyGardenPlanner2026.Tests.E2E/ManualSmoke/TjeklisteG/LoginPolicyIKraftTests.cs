namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteG;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using Ids = SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste G2 (Login rate limit): PermitLimit 1000 → 2 (E2E-seed er 1000 via PlaywrightAppFixture,
/// appsettings 5). Limiteren er pr. IP og tæller kun POST til login-endpoints. En ny bruger uden 2FA (så
/// ét login er ét POST) forsøger 3 logins: de to første er ikke 429, det tredje er 429. Gendannelsen sker
/// i en ny circuit i A's eksisterende context, så den ikke selv kræver login og dermed ikke kan blokeres.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class LoginPolicyIKraftTests(PlaywrightAppFixture fixture)
{
    // PlaywrightAppFixture sætter LoginRateLimit__PermitLimit = 1000.
    private const int OriginalPermit = 1000;
    private const int ChangedPermit = 2;
    private const int TooManyRequests = 429;

    [Fact]
    public async Task PermitLimit_AendretTil2_Blokerer3DeLoginforsoegMed429()
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var policyAdminId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, policyAdmin.Email);

        // Brugeren oprettes før ændringen (registrering er ikke et beskyttet login-endpoint).
        var userPage = await fixture.NewPageAsync();
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(userPage, fixture.RootUri);

        var policyPage = await SecurityPolicyPage.OpenAsync(
            fixture, policyAdmin, SecurityPolicyPage.LoginRateLimit, Ids.LoginPermit);
        await SecurityPolicyPage.ExpectFieldValueAsync(policyPage, Ids.LoginPermit, OriginalPermit);

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                policyPage, policyAdmin, Ids.LoginPermit, ChangedPermit,
                SecurityPolicyPage.LoginRateLimit.SavedText);
            await SecurityAlerts.ExpectPolicyChangedAsync(
                fixture.SmtpServer, policyAdminId, SecurityPolicyPage.LoginRateLimit.PolicyName);

            var first = await LoginFlow.AttemptStatusAsync(userPage, fixture.RootUri, user);
            var second = await LoginFlow.AttemptStatusAsync(userPage, fixture.RootUri, user);
            var third = await LoginFlow.AttemptStatusAsync(userPage, fixture.RootUri, user);

            first.Should().NotBe(TooManyRequests, "1. loginforsøg ligger inden for den nye grænse (2)");
            second.Should().NotBe(TooManyRequests, "2. loginforsøg ligger inden for den nye grænse (2)");
            third.Should().Be(TooManyRequests, "3. loginforsøg i samme vindue overskrider den nye grænse (2)");
        }
        finally
        {
            await SecurityPolicyPage.RestoreFieldAsync(
                fixture, policyPage, SecurityPolicyPage.LoginRateLimit, Ids.LoginPermit,
                policyAdmin, OriginalPermit);
        }
    }
}
namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteH;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste H1: 6 hurtige loginforsøg med standardgrænsen 5 giver 429 ved 6. Login-limiteren er et fixed
/// window pr. IP og tæller kun beskyttede auth-POST'er. E2E-seed er 1000, så en ny PolicyAdmin sætter
/// PermitLimit = 5 (step-up). Ny bruger A (uden 2FA, oprettet før ændringen) logger ind 6 gange med korrekt
/// password (ryd cookies mellem forsøgene), og HTTP-statussen på hver POST læses. OnRejected skriver ingen
/// body, så skærmbilledet dokumenterer, hvad brugeren faktisk ser ved 429 (efter 6. forsøg, FØR assertions).
/// Gendannelsen sker straks i en ny circuit i PolicyAdmins eksisterende context (intet login, så den kan ikke
/// blokeres), og limiteren genopbygges, så A kan logge ind igen umiddelbart efter.
/// Skærmbilleder: RateLimiting_H1_&lt;bredde&gt;. Se også G2f (grænse 2), som dækker mekanismen.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class LoginRateLimitTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste H";
    private const string TestName = "RateLimiting";

    // PlaywrightAppFixture sætter LoginRateLimit__PermitLimit = 1000 (appsettings: 5).
    private const int OriginalPermit = 1000;
    private const int ChangedPermit = 5;
    private const int Attempts = 6;
    private const int TooManyRequests = 429;

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task SekshurtigeLogin_MedGraense5_Giver429VedSjetteOgGemmerSkaermbillede(int width)
    {
        var policyAdmin = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);

        // A oprettes før ændringen (registrering er ikke et beskyttet login-endpoint).
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        var policyPage = await SecurityPolicyPage.OpenAsync(
            fixture, policyAdmin, SecurityPolicyPage.LoginRateLimit, Ids.LoginPermit);
        await SecurityPolicyPage.ExpectFieldValueAsync(policyPage, Ids.LoginPermit, OriginalPermit);

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                policyPage, policyAdmin, Ids.LoginPermit, ChangedPermit,
                SecurityPolicyPage.LoginRateLimit.SavedText);

            var statuses = new List<int>();
            for (var attempt = 1; attempt <= Attempts; attempt++)
            {
                statuses.Add(await LoginFlow.AttemptStatusAsync(page, fixture.RootUri, user));
            }

            // Skærmbilledet tages før assertions, så det også findes, hvis forventningen ikke holder.
            await page.WaitForLoadStateAsync(LoadState.Load);
            await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "H1");

            statuses.Take(ChangedPermit).Should().NotContain(
                TooManyRequests, "forsøg 1-{0} ligger inden for grænsen. Statusser: {1}",
                ChangedPermit, string.Join(", ", statuses));
            statuses[^1].Should().Be(
                TooManyRequests, "6. forsøg overskrider grænsen ({0}). Statusser: {1}",
                ChangedPermit, string.Join(", ", statuses));
        }
        finally
        {
            await SecurityPolicyPage.RestoreFieldAsync(
                fixture, policyPage, SecurityPolicyPage.LoginRateLimit, Ids.LoginPermit,
                policyAdmin, OriginalPermit);
        }

        // Gendannelsen genopbygger limiteren (ny tæller), så A kan logge ind igen med det samme.
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }
}
namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteD;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Diagnostics;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste D3: korrekt password + TOTP gennemfører handlingen. Ny PolicyAdmin pr. test. Ændrer
/// retention på fanen AuditLog-eksport og gendanner den (ny circuit efter reload kræver ny step-up).
/// Udløbstesten venter ca. ét minut (ReAuthenticationPolicy:MaxAgeMinutes = 1 i E2E-appen).
/// Skærmbilleder: StepUp_AdminSecurityPolicies_Foer og _Efter (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class KorrektStepUpTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste D";
    private const string TestName = "StepUp";

    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(PlaywrightAppFixture.ReAuthMaxAgeMinutes);
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromSeconds(2);

    [GeneratedRegex("/account/manage/enableauthenticator$", RegexOptions.IgnoreCase)]
    private static partial Regex EnableAuthenticatorUrl { get; }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task KorrektPasswordOgTotp_GemmerAendringenOgViserDenEfterReload(int width)
    {
        var (page, user) = await OpenExportJobTabAsync(width);

        var original = await SecurityPolicyPage.ReadRetentionAsync(page);
        var changed = ChangedValue(original);

        await SecurityPolicyPage.SetRetentionAsync(page, changed);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AdminSecurityPolicies_Foer");

        await StepUpModal.OpenAsync(page, () => SecurityPolicyPage.SaveButton(page).ClickAsync());
        // Admin-sider kræver 2FA, så TOTP spørges altid.
        await Assertions.Expect(StepUpModal.TotpField(page)).ToBeVisibleAsync();
        await StepUpModal.CompleteAsync(page, user);

        await Assertions.Expect(SecurityPolicyPage.Success(page))
            .ToContainTextAsync(SecurityPolicyPage.ExportJobSavedText);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "AdminSecurityPolicies_Efter");

        // Reload = ny circuit: ændringen er gemt.
        await page.ReloadAsync();
        await SecurityPolicyPage.OpenExportJobTabAsync(page);
        (await SecurityPolicyPage.ReadRetentionAsync(page)).Should().Be(changed);

        // Gendan. Ny circuit, så modalen vises igen.
        await SecurityPolicyPage.SetRetentionAsync(page, original);
        await StepUpModal.OpenAsync(page, () => SecurityPolicyPage.SaveButton(page).ClickAsync());
        await StepUpModal.CompleteAsync(page, user);

        await Assertions.Expect(SecurityPolicyPage.Success(page))
            .ToContainTextAsync(SecurityPolicyPage.ExportJobSavedText);
        (await SecurityPolicyPage.ReadRetentionAsync(page)).Should().Be(original);
    }

    [Fact]
    public async Task StepUp_IndenForVinduet_GiverIngenModal_OgEfterUdloebVisesModalIgen()
    {
        var (page, user) = await OpenExportJobTabAsync(Viewports.Desktop);

        var original = await SecurityPolicyPage.ReadRetentionAsync(page);
        var changed = ChangedValue(original);

        // 1. Step-up og gem.
        await SecurityPolicyPage.SetRetentionAsync(page, changed);
        await StepUpModal.OpenAsync(page, () => SecurityPolicyPage.SaveButton(page).ClickAsync());
        await StepUpModal.CompleteAsync(page, user);
        var sinceStepUp = Stopwatch.StartNew();
        await Assertions.Expect(SecurityPolicyPage.Success(page))
            .ToContainTextAsync(SecurityPolicyPage.ExportJobSavedText);

        // 2. Inden for vinduet: fanehop rydder statusbeskeden, så en ny besked beviser en ny gem.
        await SecurityPolicyPage.SelectTabAsync(page, "Login rate limit");
        await SecurityPolicyPage.OpenExportJobTabAsync(page);
        (await SecurityPolicyPage.ReadRetentionAsync(page)).Should().Be(changed);

        await SecurityPolicyPage.SetRetentionAsync(page, original);
        await SecurityPolicyPage.SaveButton(page).ClickAsync();

        await Assertions.Expect(SecurityPolicyPage.Success(page))
            .ToContainTextAsync(SecurityPolicyPage.ExportJobSavedText);
        await Assertions.Expect(StepUpModal.Dialog(page)).ToHaveCountAsync(0);
        sinceStepUp.Elapsed.Should().BeLessThan(MaxAge, "trin 2 skal ske inden for re-auth-vinduet");

        // 3. Efter udløb i samme circuit vises modalen igen.
        var remaining = MaxAge + ExpiryMargin - sinceStepUp.Elapsed;
        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(remaining, TestContext.Current.CancellationToken);
        }

        await StepUpModal.OpenAsync(page, () => SecurityPolicyPage.SaveButton(page).ClickAsync());
        await Assertions.Expect(StepUpModal.PasswordField(page)).ToBeVisibleAsync();

        await StepUpModal.CancelButton(page).ClickAsync();
        await Assertions.Expect(StepUpModal.Dialog(page)).ToBeHiddenAsync();
    }

    [Theory]
    [InlineData(E2ERoutes.AdminAuditLog)]
    [InlineData(E2ERoutes.AdminSecurityPolicies)]
    [InlineData(E2ERoutes.AdminSubscriptions)]
    [InlineData(E2ERoutes.AdminJitRequests)]
    public async Task NoMfa_UdenToFaktor_RedirectesTilEnableAuthenticatorPaaAlleAdminsider(string route)
    {
        var page = await fixture.LoginAndGotoAsync(SmokeTestPersonas.NoMfa, route);

        await Assertions.Expect(page).ToHaveURLAsync(EnableAuthenticatorUrl);
        await SmokePageAssertions.ExpectPageAsync(page, "Configure authenticator app", "Manage your account");
        await Assertions.Expect(StepUpModal.Dialog(page)).ToHaveCountAsync(0);
    }

    /// <summary>Retention ændres med én time, væk fra minimum 1.</summary>
    private static int ChangedValue(int original) => original > 1 ? original - 1 : original + 1;

    private async Task<(IPage Page, SmokeTestUser User)> OpenExportJobTabAsync(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminSecurityPolicies, width);

        await SmokePageAssertions.ExpectPageAsync(
            page, "Sikkerhedspolicies – MyGardenPlanner", "Sikkerhedspolicies");
        await SecurityPolicyPage.OpenExportJobTabAsync(page);

        return (page, user);
    }
}
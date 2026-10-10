namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteB;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste B4: login med gendannelseskode. Ny bruger aktiverer 2FA og får 10 koder. Efter log ud og
/// password-login vælges "logge ind med en gendannelseskode" på 2FA-prompten: en gyldig kode logger ind,
/// og samme kode afvises i en ny session (engangskode). Skærmbilleder: Gendannelseskode_LoginWithRecoveryCode
/// og Gendannelseskode_LoginWithRecoveryCodeFejl (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class GendannelseskodeTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste B";
    private const string TestName = "Gendannelseskode";

    [GeneratedRegex(@"/account/loginwithrecoverycode(\?|$)", RegexOptions.IgnoreCase)]
    private static partial Regex RecoveryCodeUrl { get; }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Gendannelseskode_LoggerIndEngangsbrugOgAfvisesVedGenbrug(int width)
    {
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        var enrollment = await TwoFactorSetup.EnableAsync(page, fixture.RootUri, user);
        enrollment.RecoveryCodes.Should().HaveCount(10);
        var recoveryCode = enrollment.RecoveryCodes[0].Trim();

        // Fra 2FA-prompten til gendannelseskode-siden.
        await OpenRecoveryCodePageAsync(page, enrollment.User);
        await SmokePageAssertions.ExpectPageAsync(
            page, "Gendannelseskode – MyGardenPlanner", "Bekræft med gendannelseskode");
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "LoginWithRecoveryCode");

        // Gyldig kode logger ind.
        await SubmitRecoveryCodeAsync(page, recoveryCode);
        await Assertions.Expect(page).Not.ToHaveURLAsync(RecoveryCodeUrl);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.TwoFactorAuthentication}");
        await SmokePageAssertions.ExpectPageAsync(page, "Two-factor authentication (2FA)", "Manage your account");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Disable 2FA", Exact = true }))
            .ToBeVisibleAsync();

        // Samme kode i en ny session afvises (engangskode).
        await OpenRecoveryCodePageAsync(page, enrollment.User);
        await SubmitRecoveryCodeAsync(page, recoveryCode);

        await Assertions.Expect(page.GetByText("Error: Ugyldig gendannelseskode indtastet.")).ToBeVisibleAsync();
        await Assertions.Expect(page).ToHaveURLAsync(RecoveryCodeUrl);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "LoginWithRecoveryCodeFejl");
    }

    /// <summary>Ny session: ryd cookies, password-login og klik fra 2FA-prompten til gendannelseskode-siden.</summary>
    private async Task OpenRecoveryCodePageAsync(IPage page, SmokeTestUser user)
    {
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.SubmitPasswordAsync(page, user);

        await page.GetByRole(AriaRole.Link, new() { Name = "logge ind med en gendannelseskode" }).ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(RecoveryCodeUrl);
    }

    private static async Task SubmitRecoveryCodeAsync(IPage page, string code)
    {
        await page.GetByLabel("Gendannelseskode", new() { Exact = true }).FillAsync(code);
        await LoginFlow.LoginButton(page).ClickAsync();
    }
}
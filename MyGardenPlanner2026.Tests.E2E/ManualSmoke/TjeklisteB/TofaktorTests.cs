namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteB;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste B3: 2FA. Ny bruger logger ind, aktiverer authenticator-app (nøglen læses fra siden),
/// logger ud (ryd cookies) og ind igen: 2FA-prompten vises, en forkert kode afvises, en korrekt kode
/// logger ind. Skærmbilleder: 2FA_EnableAuthenticator, 2FA_RecoveryCodes og 2FA_LoginWith2fa (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class TofaktorTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste B";
    private const string TestName = "2FA";

    // Manage-siderne er ikke oversat endnu (engelske titler/tekster) og ligger i ManageLayout (h1).
    private const string ManageHeading = "Manage your account";
    private const string TwoFactorTitle = "Two-factor authentication (2FA)";
    private const string EnableTitle = "Configure authenticator app";

    private const string WrongCode = "000000";

    [GeneratedRegex("/account/manage/enableauthenticator$", RegexOptions.IgnoreCase)]
    private static partial Regex EnableAuthenticatorUrl { get; }

    [GeneratedRegex(@"/account/loginwith2fa(\?|$)", RegexOptions.IgnoreCase)]
    private static partial Regex LoginWith2faUrl { get; }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task AktiverToFaktor_LogUdOgInd_ViserToFaktorPromptOgAfviserForkertKode(int width)
    {
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        // 2FA-oversigten: endnu ikke aktiveret.
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.TwoFactorAuthentication}");
        await SmokePageAssertions.ExpectPageAsync(page, TwoFactorTitle, ManageHeading);
        var addLink = page.GetByRole(AriaRole.Link, new() { Name = "Add authenticator app", Exact = true });
        await Assertions.Expect(addLink).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Disable 2FA", Exact = true }))
            .ToHaveCountAsync(0);

        await addLink.ClickAsync();
        await Assertions.Expect(page).ToHaveURLAsync(EnableAuthenticatorUrl);
        await SmokePageAssertions.ExpectPageAsync(page, EnableTitle, ManageHeading);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "EnableAuthenticator");

        // Verificér med TOTP udregnet fra nøglen på siden.
        var key = await TwoFactorSetup.ReadSharedKeyAsync(page);
        var recoveryCodes = await TwoFactorSetup.VerifyAsync(page, key);

        recoveryCodes.Should().HaveCount(10);
        await Assertions.Expect(page.GetByText("Your authenticator app has been verified.")).ToBeVisibleAsync();
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "RecoveryCodes");

        // 2FA-oversigten: nu aktiveret.
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.TwoFactorAuthentication}");
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Disable 2FA", Exact = true }))
            .ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Add authenticator app", Exact = true }))
            .ToHaveCountAsync(0);

        // Log ud (ingen log ud-knap i layoutet) og ind igen: 2FA-prompten.
        await page.Context.ClearCookiesAsync();
        var enrolledUser = user with { TwoFactorEnabled = true, AuthenticatorKey = key };

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.SubmitPasswordAsync(page, enrolledUser);

        await Assertions.Expect(page).ToHaveURLAsync(LoginWith2faUrl);
        await SmokePageAssertions.ExpectPageAsync(page, "Totrinsbekræftelse – MyGardenPlanner", "Totrinsbekræftelse");
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "LoginWith2fa");

        // Forkert kode afvises og bliver på siden.
        await page.GetByLabel("Godkendelseskode").FillAsync(WrongCode);
        await LoginFlow.LoginButton(page).ClickAsync();
        await Assertions.Expect(page.GetByText("Error: Ugyldig godkendelseskode.")).ToBeVisibleAsync();
        await Assertions.Expect(page).ToHaveURLAsync(LoginWith2faUrl);

        // Korrekt kode logger ind.
        await LoginFlow.SubmitTwoFactorCodeAsync(page, enrolledUser);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.TwoFactorAuthentication}");
        await SmokePageAssertions.ExpectPageAsync(page, TwoFactorTitle, ManageHeading);
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = "Disable 2FA", Exact = true }))
            .ToBeVisibleAsync();
    }
}
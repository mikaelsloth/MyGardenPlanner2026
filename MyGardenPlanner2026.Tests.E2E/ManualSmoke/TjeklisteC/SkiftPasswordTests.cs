namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste C2: skift password. Ny bruger logger ind og skifter password på
/// /Account/Manage/ChangePassword (redirect til samme side med statusbesked). Efter skiftet virker det nye
/// password ved login og det gamle afvises. Negative tests: forkert nuværende password, forskelligt
/// bekræftet password og for kort nyt password. Skærmbilleder: SkiftPassword_ChangePassword og
/// SkiftPassword_ChangePasswordEfterSkift (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class SkiftPasswordTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste C";
    private const string TestName = "SkiftPassword";

    // Manage-siderne er ikke oversat endnu (engelske titler/tekster) og ligger i ManageLayout (h1).
    private const string PageTitle = "Change password";
    private const string ManageHeading = "Manage your account";
    private const string ChangedText = "Your password has been changed";
    private const string WrongCurrentText = "Incorrect password";

    // Login.razor.cs: "Error: Invalid login attempt." (engelsk, ikke oversat endnu).
    private const string InvalidLoginText = "Invalid login attempt";

    private const string NewPassword = "NyAdgangskode456!";
    private const string WrongPassword = "Forkert-Adgangskode1!";

    [GeneratedRegex("/account/manage/changepassword$", RegexOptions.IgnoreCase)]
    private static partial Regex ChangePasswordUrl { get; }

    public static TheoryData<int> Widths()
    {
        var data = new TheoryData<int>();
        foreach (var width in Viewports.All)
        {
            data.Add(width);
        }

        return data;
    }

    [Fact]
    public async Task SkiftPassword_MedGyldigtNytPassword_NytVirkerOgGammeltAfvises()
    {
        var (page, user) = await LoginNewUserOnChangePasswordPageAsync();

        await SubmitPasswordFormAsync(page, user.Password, NewPassword, NewPassword);

        // Redirect til samme side med statusbesked.
        await Assertions.Expect(page.GetByText(ChangedText)).ToBeVisibleAsync();
        await Assertions.Expect(page).ToHaveURLAsync(ChangePasswordUrl);

        // Nyt password kan logge ind.
        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.LoginAsync(page, user with { Password = NewPassword });
        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);

        // Gammelt password afvises.
        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.FillCredentialsAsync(page, user.Email, user.Password);
        await LoginFlow.LoginButton(page).ClickAsync();
        await Assertions.Expect(page.GetByText(InvalidLoginText)).ToBeVisibleAsync();
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task SkiftPassword_Skaermbilleder_GemmerSideOgEfterSkift(int width)
    {
        var (page, user) = await LoginNewUserOnChangePasswordPageAsync(width);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "ChangePassword");

        await SubmitPasswordFormAsync(page, user.Password, NewPassword, NewPassword);
        await Assertions.Expect(page.GetByText(ChangedText)).ToBeVisibleAsync();

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "ChangePasswordEfterSkift");
    }

    [Fact]
    public async Task SkiftPassword_ForkertNuvaerendePassword_ViserFejlOgGammeltVirkerStadig()
    {
        var (page, user) = await LoginNewUserOnChangePasswordPageAsync();

        await SubmitPasswordFormAsync(page, WrongPassword, NewPassword, NewPassword);

        await Assertions.Expect(page.GetByText(WrongCurrentText)).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByText(ChangedText)).ToHaveCountAsync(0);

        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.LoginAsync(page, user);
        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }

    [Theory]
    [InlineData("Ab1!", "Ab1!", "at least 6")]
    [InlineData(NewPassword, "AndenAdgangskode789!", "do not match")]
    public async Task SkiftPassword_UgyldigtNytPassword_ViserValideringsfejl(
        string newPassword, string confirmPassword, string expectedError)
    {
        var (page, user) = await LoginNewUserOnChangePasswordPageAsync();

        await SubmitPasswordFormAsync(page, user.Password, newPassword, confirmPassword);

        await Assertions.Expect(page.Locator(".validation-message")).ToContainTextAsync(expectedError);
        await Assertions.Expect(page.GetByText(ChangedText)).ToHaveCountAsync(0);
    }

    /// <summary>Ny bruger via UI, logget ind og stående på /Account/Manage/ChangePassword.</summary>
    private async Task<(IPage Page, SmokeTestUser User)> LoginNewUserOnChangePasswordPageAsync(
        int width = Viewports.Desktop)
    {
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.ChangePassword}");
        await SmokePageAssertions.ExpectPageAsync(page, PageTitle, ManageHeading);

        return (page, user);
    }

    private static async Task SubmitPasswordFormAsync(
        IPage page, string oldPassword, string newPassword, string confirmPassword)
    {
        await page.GetByLabel("Old password", new() { Exact = true }).FillAsync(oldPassword);
        await page.GetByLabel("New password", new() { Exact = true }).FillAsync(newPassword);
        await page.GetByLabel("Confirm password", new() { Exact = true }).FillAsync(confirmPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Update password", Exact = true }).ClickAsync();
    }

    /// <summary>Log ud (ingen log ud-knap i layoutet: ryd cookies) og åbn login-siden.</summary>
    private async Task OpenLoginAsLoggedOutAsync(IPage page)
    {
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
    }
}
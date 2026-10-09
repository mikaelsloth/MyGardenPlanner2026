namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteB;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste B2: forkert login. Login.razor.cs bruger lockoutOnFailure: false, så 5 forkerte
/// forsøg låser IKKE kontoen (tracker/sikkerhedsalarm testes i PR3). /Account/Lockout nås kun
/// for en allerede låst bruger, som her låses direkte i databasen. Skærmbilleder:
/// ForkertLogin_Lockout_&lt;bredde&gt;.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class ForkertLoginTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste B";
    private const string TestName = "ForkertLogin";

    private const int WrongAttempts = 5;
    private const string WrongPassword = "Forkert-Adgangskode1!";

    // Login.razor.cs: "Error: Invalid login attempt." (engelsk, ikke oversat endnu).
    private const string InvalidLoginText = "Invalid login attempt";

    [GeneratedRegex("/account/lockout$", RegexOptions.IgnoreCase)]
    private static partial Regex LockoutUrl { get; }

    [Fact(Skip = "Afventer PR3: 5. forsøg udløser sikkerhedsalarm via SMTP (SmtpException uden SMTP-server).")]
    public async Task ForkertPassword_FemGange_ViserFejlHverGangOgKontoenLaasesIkke()
    {
        var page = await fixture.NewPageAsync();
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        for (var attempt = 1; attempt <= WrongAttempts; attempt++)
        {
            // Frisk side pr. forsøg, så fejlteksten fra forrige forsøg ikke forveksles med det nye svar.
            await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
            await LoginFlow.FillCredentialsAsync(page, user.Email, WrongPassword);
            await LoginFlow.LoginButton(page).ClickAsync();

            await Assertions.Expect(page.GetByText(InvalidLoginText)).ToBeVisibleAsync();
            page.Url.Should().NotContainEquivalentOf(
                E2ERoutes.Lockout, "forsøg {0} må ikke låse kontoen (lockoutOnFailure: false)", attempt);
        }

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task LaastBruger_Login_RedirectesTilLockoutOgGemmerSkaermbillede(int width)
    {
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);
        await SmokeTestDataSeeder.LockOutUserAsync(fixture.AppConnectionString, user.Email);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        await Assertions.Expect(page).ToHaveURLAsync(LockoutUrl);
        await SmokePageAssertions.ExpectPageAsync(page, "Konto låst – MyGardenPlanner", "Konto låst");
        await Assertions.Expect(page.Locator(".status-message.status-danger"))
            .ToContainTextAsync("Denne konto er blevet låst.");

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "Lockout");
    }
}
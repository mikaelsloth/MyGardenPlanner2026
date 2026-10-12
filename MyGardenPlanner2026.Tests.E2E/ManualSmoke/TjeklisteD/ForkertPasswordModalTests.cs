namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteD;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste D2: forkert password i step-up modal. Hver test får en ny PolicyAdmin, så fejltælleren
/// (alarm ved præcis Threshold = 5) er isoleret. Modalen åbnes via "Gem" på fanen AuditLog-eksport.
/// Skærmbilleder: StepUp_AdminSecurityPolicies_ForkertPassword (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class ForkertPasswordModalTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste D";
    private const string TestName = "StepUp";

    private const int Threshold = 5;
    private const string WrongPassword = "Forkert-Adgangskode1!";
    private const string WrongCode = "000000";

    private const string WrongPasswordText = "Error: Forkert adgangskode.";
    private const string MissingCodeText = "Error: Indtast venligst din godkendelseskode.";
    private const string InvalidCodeText = "Error: Ugyldig godkendelseskode.";

    public static TheoryData<int> Widths()
    {
        var data = new TheoryData<int>();
        foreach (var width in Viewports.All)
        {
            data.Add(width);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task ForkertPassword_ViserFejlOgGemmerSkaermbillede(int width)
    {
        var (page, _) = await OpenModalAsync(width);

        await SubmitAsync(page, WrongPassword);

        await Assertions.Expect(StepUpModal.Error(page)).ToContainTextAsync(WrongPasswordText);
        await Assertions.Expect(StepUpModal.Dialog(page)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".status-success")).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(
            page, Chapter, Checklist, TestName, "AdminSecurityPolicies_ForkertPassword");
    }

    [Fact]
    public async Task TomOgForkertTotp_MedKorrektPassword_ViserHverSinFejl()
    {
        var (page, user) = await OpenModalAsync();

        await SubmitAsync(page, user.Password);
        await Assertions.Expect(StepUpModal.Error(page)).ToContainTextAsync(MissingCodeText);

        await SubmitAsync(page, user.Password, WrongCode);
        await Assertions.Expect(StepUpModal.Error(page)).ToContainTextAsync(InvalidCodeText);

        await Assertions.Expect(StepUpModal.Dialog(page)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator(".status-success")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ForkerteForsoeg_FemGange_TaellesOgSenderSikkerhedsalarmVedFemte()
    {
        var (page, user) = await OpenModalAsync();
        var userId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, user.Email);

        // Skiftevis forkert password og forkert TOTP: begge tælles, og den nye fejltekst
        // adskiller sig fra den forrige, så hvert svar kan afventes uden race.
        for (var attempt = 1; attempt <= Threshold; attempt++)
        {
            var wrongPasswordAttempt = attempt % 2 == 1;

            if (wrongPasswordAttempt)
            {
                await SubmitAsync(page, WrongPassword);
            }
            else
            {
                await SubmitAsync(page, user.Password, WrongCode);
            }

            await Assertions.Expect(StepUpModal.Error(page))
                .ToContainTextAsync(wrongPasswordAttempt ? WrongPasswordText : InvalidCodeText);

            if (attempt < Threshold)
            {
                fixture.SmtpServer.Messages.Should().NotContain(
                    m => m.Body.Contains(userId, StringComparison.Ordinal),
                    "forsøg {0} ligger under tærsklen for sikkerhedsalarm", attempt);
            }
        }

        var alert = await fixture.SmtpServer.WaitForMessageAsync(
            m => m.Body.Contains(userId, StringComparison.Ordinal));

        alert.Recipients.Should().Equal(PlaywrightAppFixture.SecurityAlertRecipient);
        alert.Subject.Should().Be(PlaywrightAppFixture.SecurityAlertSubject);
        // IP-værdien asserteres ikke: HttpContext er normalt null i en circuit.
        alert.Body.Should().Contain($"Bruger-ID: {userId}").And.Contain("IP-adresse:");
    }

    /// <summary>Ny PolicyAdmin, logget ind på security-policies med modalen åben fra fanen AuditLog-eksport.</summary>
    private async Task<(IPage Page, SmokeTestUser User)> OpenModalAsync(int width = Viewports.Desktop)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminSecurityPolicies, width);

        await SmokePageAssertions.ExpectPageAsync(
            page, "Sikkerhedspolicies – MyGardenPlanner", "Sikkerhedspolicies");

        // Fanen er også "klar"-signalet: uden et forbundet kredsløb ignoreres klik.
        // Fanen er også "klar"-signalet: uden et forbundet kredsløb ignoreres klik.
        var tab = page.GetByRole(AriaRole.Tab, new() { Name = "AuditLog-eksport" });
        await BlazorInteraction.ActUntilAsync(
            () => tab.ClickAsync(),
            () => IsActiveAsync(tab),
            maxAttempts: 12,
            description: "Fanen AuditLog-eksport blev ikke valgt");

        await Assertions.Expect(page.Locator("#exportjob-retention")).ToBeVisibleAsync();

        await StepUpModal.OpenAsync(
            page, () => page.GetByRole(AriaRole.Button, new() { Name = "Gem", Exact = true }).ClickAsync());

        return (page, user);
    }

    /// <summary>Tom TOTP rydder feltet, så en tidligere kode ikke genbruges.</summary>
    private static async Task SubmitAsync(IPage page, string password, string? code = null)
    {
        await StepUpModal.PasswordField(page).FillAsync(password);
        await StepUpModal.TotpField(page).FillAsync(code ?? string.Empty);
        await StepUpModal.ConfirmButton(page).ClickAsync();
    }

    /// <summary>Valgt fane har klassen "active" (aria-selected er en bool-attribut uden værdi).</summary>
    private static async Task<bool> IsActiveAsync(ILocator tab)
    {
        var classes = await tab.GetAttributeAsync("class") ?? string.Empty;
        return classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("active");
    }
}
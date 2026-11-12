namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text;

/// <summary>
/// Tjekliste C1: skift e-mail. Ny bruger logger ind, skifter e-mail på /Account/Manage/Email og følger
/// bekræftelseslinket (vises på siden, fordi appen bruger IdentityNoOpEmailSender). Efter bekræftelse
/// virker den nye e-mail ved login og den gamle afvises. Negative tests: uændret e-mail, ugyldig e-mail
/// og ugyldigt link. Skærmbilleder: SkiftEmail_Email og SkiftEmail_EmailEfterSkift (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class SkiftEmailTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste C";
    private const string TestName = "SkiftEmail";

    // Manage-siderne ligger i ManageLayout (h1) og har titlen "<side> – MyGardenPlanner".
    private const string ManageTitle = "Administrer e-mail – MyGardenPlanner";
    private const string ManageHeading = "Administrer din konto";
    private const string ChangeSentText = "Bekræftelseslink til skift af e-mail er sendt. Tjek din e-mail.";
    private const string UnchangedText = "Din e-mail er uændret.";
    private const string ConfirmLinkText = "Klik her for at bekræfte din nye e-mail";

    // ConfirmEmailChange er oversat og ligger i PublicLayout (AuthPageShell: h1 = Title).
    private const string ConfirmTitle = "Bekræft e-mailskift – MyGardenPlanner";
    private const string ConfirmHeading = "Bekræft e-mailskift";
    private const string ConfirmedText = "Tak, fordi du bekræftede skiftet af din e-mail.";
    private const string ConfirmFailedText = "Error: Der opstod en fejl ved skift af e-mail.";

    // Login.razor.cs: "Error: Ugyldigt loginforsøg."
    private const string InvalidLoginText = "Ugyldigt loginforsøg";

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
    public async Task SkiftEmail_MedBekraeftelseslink_NyEmailVirkerOgGammelAfvises()
    {
        var (page, user) = await LoginNewUserOnEmailPageAsync();
        var newEmail = RegisteredUserFlow.NewEmail();

        await SubmitNewEmailAsync(page, newEmail);
        await Assertions.Expect(page.GetByText(ChangeSentText)).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = ConfirmLinkText }).ClickAsync();
        await SmokePageAssertions.ExpectPageAsync(page, ConfirmTitle, ConfirmHeading);
        await Assertions.Expect(page.GetByText(ConfirmedText)).ToBeVisibleAsync();

        // Ny e-mail kan logge ind.
        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.LoginAsync(page, user with { Email = newEmail });
        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);

        // Gammel e-mail afvises.
        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.FillCredentialsAsync(page, user.Email, user.Password);
        await LoginFlow.LoginButton(page).ClickAsync();
        await Assertions.Expect(page.GetByText(InvalidLoginText)).ToBeVisibleAsync();
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task SkiftEmail_Skaermbilleder_GemmerSideOgEfterSkift(int width)
    {
        var (page, _) = await LoginNewUserOnEmailPageAsync(width);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "Email");

        await SubmitNewEmailAsync(page, RegisteredUserFlow.NewEmail());
        await Assertions.Expect(page.GetByText(ChangeSentText)).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = ConfirmLinkText })).ToBeVisibleAsync();

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "EmailEfterSkift");
    }

    [Fact]
    public async Task SkiftEmail_UaendretEmail_ViserBeskedUdenBekraeftelseslink()
    {
        var (page, _) = await LoginNewUserOnEmailPageAsync();

        // Feltet "Ny e-mail" er forudfyldt med den nuværende e-mail.
        await page.GetByRole(AriaRole.Button, new() { Name = "Skift e-mail", Exact = true }).ClickAsync();

        await Assertions.Expect(page.GetByText(UnchangedText)).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = ConfirmLinkText })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task SkiftEmail_UgyldigEmail_ViserValideringsfejlUdenBekraeftelseslink()
    {
        var (page, _) = await LoginNewUserOnEmailPageAsync();

        await SubmitNewEmailAsync(page, "ikke-en-email");

        await Assertions.Expect(page.Locator(".validation-message"))
            .ToContainTextAsync("Angiv en gyldig e-mailadresse.");
        await Assertions.Expect(page.GetByText(ChangeSentText)).ToHaveCountAsync(0);
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new() { Name = ConfirmLinkText })).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task SkiftEmail_KodeMedUgyldigtFormat_ViserFejlUdenServerfejlOgGammelEmailVirker()
    {
        var (page, user) = await LoginNewUserOnEmailPageAsync();
        var newEmail = RegisteredUserFlow.NewEmail();
        var userId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, user.Email);

        // Ikke gyldigt Base64Url (R2.3): før rettelsen gav det en serverfejl (500).
        var response = await page.GotoAsync(
            $"{fixture.RootUri}{E2ERoutes.ConfirmEmailChange}" +
            $"?userId={Uri.EscapeDataString(userId)}&email={Uri.EscapeDataString(newEmail)}" +
            $"&code={Uri.EscapeDataString("!!!")}");

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        await SmokePageAssertions.ExpectPageAsync(page, ConfirmTitle, ConfirmHeading);
        await Assertions.Expect(page.GetByText(ConfirmFailedText)).ToBeVisibleAsync();

        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.LoginAsync(page, user);
        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }

    [Fact]
    public async Task SkiftEmail_UgyldigtBekraeftelseslink_AfvisesOgEmailErUaendret()
    {
        var (page, user) = await LoginNewUserOnEmailPageAsync();
        var newEmail = RegisteredUserFlow.NewEmail();
        var userId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, user.Email);

        // Gyldigt Base64Url-format, men forkert token.
        var wrongCode = Base64Url("forkert-kode");
        await page.GotoAsync(
            $"{fixture.RootUri}{E2ERoutes.ConfirmEmailChange}" +
            $"?userId={Uri.EscapeDataString(userId)}&email={Uri.EscapeDataString(newEmail)}&code={wrongCode}");

        await SmokePageAssertions.ExpectPageAsync(page, ConfirmTitle, ConfirmHeading);
        await Assertions.Expect(page.GetByText(ConfirmFailedText)).ToBeVisibleAsync();

        // Den nye e-mail virker ikke ...
        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.FillCredentialsAsync(page, newEmail, user.Password);
        await LoginFlow.LoginButton(page).ClickAsync();
        await Assertions.Expect(page.GetByText(InvalidLoginText)).ToBeVisibleAsync();

        // ... og den gamle virker stadig.
        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.LoginAsync(page, user);
        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }

    /// <summary>Ny bruger via UI, logget ind og stående på /Account/Manage/Email.</summary>
    private async Task<(IPage Page, SmokeTestUser User)> LoginNewUserOnEmailPageAsync(int width = Viewports.Desktop)
    {
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.ManageEmail}");
        await SmokePageAssertions.ExpectPageAsync(page, ManageTitle, ManageHeading);

        return (page, user);
    }

    private static async Task SubmitNewEmailAsync(IPage page, string newEmail)
    {
        // Exact = true: label "E-mail" (det deaktiverede felt) er en delstreng af "Ny e-mail".
        await page.GetByLabel("Ny e-mail", new() { Exact = true }).FillAsync(newEmail);
        await page.GetByRole(AriaRole.Button, new() { Name = "Skift e-mail", Exact = true }).ClickAsync();
    }

    /// <summary>Log ud (ingen log ud-knap i layoutet: ryd cookies) og åbn login-siden.</summary>
    private async Task OpenLoginAsLoggedOutAsync(IPage page)
    {
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
    }

    private static string Base64Url(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
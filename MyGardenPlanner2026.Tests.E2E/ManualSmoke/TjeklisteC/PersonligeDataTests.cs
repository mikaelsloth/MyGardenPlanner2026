namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste C3: download/slet personlige data. Ny bruger logger ind, henter PersonalData.json fra
/// /Account/Manage/PersonalData og sletter derefter kontoen via /Account/Manage/DeletePersonalData.
/// Efter sletning afvises login. Negativ test: forkert password ved sletning. Skærmbilleder:
/// PersonligeData_PersonalData og PersonligeData_DeletePersonalData (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class PersonligeDataTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste C";
    private const string TestName = "PersonligeData";

    // Manage-siderne ligger i ManageLayout (h1) og har titlen "<side> – MyGardenPlanner".
    private const string PersonalDataTitle = "Personlige data – MyGardenPlanner";
    private const string DeleteTitle = "Slet personlige data – MyGardenPlanner";
    private const string ManageHeading = "Administrer din konto";

    private const string DownloadFileName = "PersonalData.json";
    private const string DeleteButtonName = "Slet data og luk min konto";
    private const string IncorrectPasswordText = "Adgangskoden er forkert";

    // Login.razor.cs: "Error: Ugyldigt loginforsøg."
    private const string InvalidLoginText = "Ugyldigt loginforsøg";

    private const string WrongPassword = "Forkert-Adgangskode1!";

    [GeneratedRegex("/account/manage/deletepersonaldata$", RegexOptions.IgnoreCase)]
    private static partial Regex DeleteUrl { get; }

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
    public async Task DownloadPersonligeData_Knap_HenterPersonalDataJsonMedBrugerensEmail()
    {
        var (page, user) = await LoginNewUserOnPersonalDataPageAsync();

        var download = await page.RunAndWaitForDownloadAsync(() =>
            page.GetByRole(AriaRole.Button, new() { Name = "Download", Exact = true }).ClickAsync());

        download.SuggestedFilename.Should().Be(DownloadFileName);

        var path = await download.PathAsync()
            ?? throw new InvalidOperationException("Download gav ingen filsti.");
        using var json = JsonDocument.Parse(
            await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));

        json.RootElement.GetProperty("Email").GetString().Should().Be(user.Email);
        // Authenticator-nøglen er en hemmelighed og eksporteres ikke (R2.5).
        json.RootElement.TryGetProperty("Authenticator Key", out _).Should().BeFalse();
    }

    [Fact]
    public async Task SletPersonligeData_MedKorrektPassword_KontoenSlettesOgLoginAfvises()
    {
        var (page, user) = await LoginNewUserOnPersonalDataPageAsync();
        await OpenDeletePageAsync(page);

        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(user.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = DeleteButtonName, Exact = true }).ClickAsync();

        // D4-2: efter sletning redirectes til forsiden, og brugeren er logget ud.
        await Assertions.Expect(page).ToHaveURLAsync($"{fixture.RootUri}/");
        await Assertions.Expect(page.GetByRole(AriaRole.Button, new() { Name = DeleteButtonName, Exact = true }))
            .ToHaveCountAsync(0);

        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.FillCredentialsAsync(page, user.Email, user.Password);
        await LoginFlow.LoginButton(page).ClickAsync();
        await Assertions.Expect(page.GetByText(InvalidLoginText)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SletPersonligeData_ForkertPassword_ViserFejlOgKontoenFindesStadig()
    {
        var (page, user) = await LoginNewUserOnPersonalDataPageAsync();
        await OpenDeletePageAsync(page);

        await page.GetByLabel("Adgangskode", new() { Exact = true }).FillAsync(WrongPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = DeleteButtonName, Exact = true }).ClickAsync();

        await Assertions.Expect(page.GetByText(IncorrectPasswordText)).ToBeVisibleAsync();

        await OpenLoginAsLoggedOutAsync(page);
        await LoginFlow.LoginAsync(page, user);
        page.Url.Should().NotContainEquivalentOf(E2ERoutes.Login);
    }

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task PersonligeData_Skaermbilleder_GemmerPersonalDataOgDeletePersonalData(int width)
    {
        var (page, _) = await LoginNewUserOnPersonalDataPageAsync(width);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "PersonalData");

        await OpenDeletePageAsync(page);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "DeletePersonalData");
    }

    /// <summary>Ny bruger via UI, logget ind og stående på /Account/Manage/PersonalData.</summary>
    private async Task<(IPage Page, SmokeTestUser User)> LoginNewUserOnPersonalDataPageAsync(
        int width = Viewports.Desktop)
    {
        var page = await fixture.NewPageAsync(width);
        var user = await RegisteredUserFlow.RegisterAndConfirmAsync(page, fixture.RootUri);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
        await LoginFlow.LoginAsync(page, user);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.PersonalData}");
        await SmokePageAssertions.ExpectPageAsync(page, PersonalDataTitle, ManageHeading);

        return (page, user);
    }

    /// <summary>Fra PersonalData via linket "Delete" til DeletePersonalData.</summary>
    private static async Task OpenDeletePageAsync(IPage page)
    {
        await page.GetByRole(AriaRole.Link, new() { Name = "Slet", Exact = true }).ClickAsync();

        await Assertions.Expect(page).ToHaveURLAsync(DeleteUrl);
        await SmokePageAssertions.ExpectPageAsync(page, DeleteTitle, ManageHeading);
    }

    /// <summary>Log ud (ingen log ud-knap i layoutet: ryd cookies) og åbn login-siden.</summary>
    private async Task OpenLoginAsLoggedOutAsync(IPage page)
    {
        await page.Context.ClearCookiesAsync();
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.Login}");
    }
}
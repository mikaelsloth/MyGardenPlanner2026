namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteC;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.Json;

/// <summary>
/// Tjekliste C4: download af backup før kontosletning (R10.2/R10.14). Smoke-brugeren logger ind, åbner
/// /Account/Manage/DeletePersonalData og henter MyGardenPlanner-backup.json. Testen verificerer filens
/// format, ikke signaturen: E2E-appen kører i en separat proces med egen nøglering (signaturen dækkes af
/// AccountBackupServiceTests og PersistedDataProtectionTests i Tests.Unit). Ingen skærmbilleder: C3 viser
/// kortet på DeletePersonalData ved alle bredder.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class BackupDownloadTests(PlaywrightAppFixture fixture)
{
    private const string BackupCardTitle = "Hent en backup, før du sletter";
    private const string BackupButtonName = "Download backup";
    private const string BackupFileName = "MyGardenPlanner-backup.json";
    private const string BackupFormat = "MyGardenPlanner.AccountBackup";

    [Fact]
    public async Task DownloadBackup_Knap_HenterJsonMedKuvertFormatVersionHashOgSignatur()
    {
        var page = await OpenDeletePageAsync(SmokeTestPersonas.Plain);

        using var json = await DownloadBackupAsync(page);
        var root = json.RootElement;

        root.GetProperty("format").GetString().Should().Be(BackupFormat);
        root.GetProperty("version").GetInt32().Should().Be(1);
        root.GetProperty("createdUtc").GetDateTimeOffset()
            .Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));
        root.GetProperty("emailHash").GetString().Should().MatchRegex("^[0-9a-fA-F]{64}$");
        root.GetProperty("signature").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DownloadBackup_Indhold_HarBrugerensEmailMenIngenAuthenticatorNoegle()
    {
        var user = fixture.SmokeTestUsers[SmokeTestPersonas.Plain];
        var page = await OpenDeletePageAsync(SmokeTestPersonas.Plain);

        using var json = await DownloadBackupAsync(page);
        var personalData = json.RootElement.GetProperty("payload").GetProperty("personalData");

        personalData.GetProperty("Email").GetString().Should().Be(user.Email);
        // Authenticator-nøglen er en hemmelighed og eksporteres ikke (samme regel som PersonalData.json, R2.5).
        personalData.TryGetProperty("Authenticator Key", out _).Should().BeFalse();
    }

    [Fact]
    public async Task DownloadBackup_Haver_ErEnListe()
    {
        var page = await OpenDeletePageAsync(SmokeTestPersonas.Plain);

        using var json = await DownloadBackupAsync(page);

        json.RootElement.GetProperty("payload").GetProperty("gardens").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task DownloadBackup_EmailHash_ErStabilPrBrugerOgForskelligMellemBrugere()
    {
        var plainPage = await OpenDeletePageAsync(SmokeTestPersonas.Plain);
        using var first = await DownloadBackupAsync(plainPage);
        using var second = await DownloadBackupAsync(plainPage);

        var otherPage = await OpenDeletePageAsync(SmokeTestPersonas.NoMfa);
        using var other = await DownloadBackupAsync(otherPage);

        var hash = EmailHash(first);
        EmailHash(second).Should().Be(hash);
        EmailHash(other).Should().NotBe(hash);
    }

    [Theory]
    [InlineData(Viewports.Mobile)]
    [InlineData(Viewports.BelowBreakpoint)]
    public async Task DownloadBackup_SmaaViewports_KortetVisesOgDownloadVirker(int width)
    {
        var page = await OpenDeletePageAsync(SmokeTestPersonas.Plain, width);

        await Assertions.Expect(page.GetByText(BackupCardTitle, new() { Exact = true })).ToBeVisibleAsync();
        await Assertions.Expect(BackupButton(page)).ToBeVisibleAsync();

        using var json = await DownloadBackupAsync(page);

        json.RootElement.GetProperty("format").GetString().Should().Be(BackupFormat);
    }

    /// <summary>Logger ind som persona og lander på DeletePersonalData (ReturnUrl).</summary>
    private async Task<IPage> OpenDeletePageAsync(string persona, int? width = null)
    {
        var page = await fixture.LoginAndGotoAsync(persona, E2ERoutes.DeletePersonalData, width);
        await SmokePageAssertions.ExpectPageAsync(page, ManagePageTexts.DeleteTitle, ManagePageTexts.ManageHeading);
        return page;
    }

    private static ILocator BackupButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = BackupButtonName, Exact = true });

    /// <summary>Klikker "Download backup", kontrollerer filnavnet og returnerer den indlæste JSON. Kalderen disposer.</summary>
    private static async Task<JsonDocument> DownloadBackupAsync(IPage page)
    {
        var download = await page.RunAndWaitForDownloadAsync(() => BackupButton(page).ClickAsync());

        download.SuggestedFilename.Should().Be(BackupFileName);

        var path = await download.PathAsync()
            ?? throw new InvalidOperationException("Download gav ingen filsti.");

        return JsonDocument.Parse(await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken));
    }

    private static string? EmailHash(JsonDocument json) =>
        json.RootElement.GetProperty("emailHash").GetString();
}
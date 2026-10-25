namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteJ;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Web;

/// <summary>
/// Tjekliste J4 (volumen): grænserne for eksport. Kører i egen collection (egen app og database), fordi
/// datasættet på 250.001 rækker ellers ville få G2c (baggrundseksport med tomt filter) til at ramme grænsen.
///
/// Datasæt (én e-mail, én entitet, alle rækker en dag har samme tidspunkt kl. 12:00 UTC):
///   D0: 50.000 | D1: 1 | D2: 199.999 | D3: 1
/// Filtre (e-mail + datoer): D0-D0 = 50.000, D0-D1 = 50.001, D0-D2 = 250.000, ingen datoer = 250.001.
///
/// 250.000 rækker downloades ikke (bekræftelsen verificeres og annulleres): selve download-flowet er dækket
/// ved 50.000 og 50.001. Skærmbilleder: Auditlog_J4_Bekraeftelse og Auditlog_J4_Graense (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightVolumeAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AuditLogEksportVolumenTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste J";
    private const string TestName = "Auditlog";

    private const int Rows50000 = 50_000;
    private const int Rows50001 = 50_001;
    private const int Rows250000 = 250_000;
    private const int Rows250001 = 250_001;

    private const float SearchTimeoutMs = 30_000;
    private const float FetchTimeoutMs = 180_000;

    private static readonly TimeSpan JobTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan JobPoll = TimeSpan.FromSeconds(3);

    // Seedes én gang pr. app-instans (testene i klassen kører efter hinanden).
    private static readonly ConcurrentDictionary<string, Lazy<Task<Dataset>>> Datasets = new();

    private sealed record Dataset(string Email, string Entity, DateOnly Day0)
    {
        public DateOnly Day(int offset) => Day0.AddDays(offset);
    }

    // --- 50.000: ingen bekræftelse ---

    [Fact]
    public async Task Eksport_50000Raekker_StarterDownloadUdenBekraeftelse()
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        await SearchAsync(page, data, data.Day(0), data.Day(0), Rows50000);

        var url = await ExportAsync(page, user);

        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToHaveCountAsync(0);
        await Assertions.Expect(AuditLogPage.ExportError(page)).ToHaveCountAsync(0);

        var response = await AuditLogExport.FetchAsync(page, url, FetchTimeoutMs);

        response.Status.Should().Be(200);
        var rows = AuditLogExport.ParseCsv(response.Text);
        rows.Should().HaveCount(Rows50000 + 1, "overskrift + 50.000 rækker");
        rows.Skip(1).Select(r => r[2]).Distinct().Should().Equal(data.Email);
        rows.Skip(1).Select(r => r[6]).Distinct().Should().HaveCount(Rows50000, "EntityId er 1..50.000");
    }

    // --- 50.001: bekræftelse ---

    [Fact]
    public async Task Eksport_50001Raekker_KraeverBekraeftelse_AnnullerGiverIntet_JaFortsaetDownloader()
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        await SearchAsync(page, data, data.Day(0), data.Day(1), Rows50001);

        // Eksportér: bekræftelsen vises, og intet download startes.
        await ClickExportAsync(page, user, () => AuditLogPage.ThresholdConfirm(page).IsVisibleAsync());

        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page))
            .ToContainTextAsync("Eksporten omfatter 50001 rækker og kan tage et øjeblik. Fortsæt?");
        (await AuditLogExport.OpenedUrlsAsync(page)).Should().BeEmpty();

        // Annullér: bekræftelsen forsvinder, og intet download startes.
        await AuditLogPage.ThresholdCancelButton(page).ClickAsync();
        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToHaveCountAsync(0);

        var openedAfterCancel = await BlazorInteraction.WaitUntilAsync(
            async () => (await AuditLogExport.OpenedUrlsAsync(page)).Count > 0, TimeSpan.FromSeconds(1));
        openedAfterCancel.Should().BeFalse("Annullér må ikke starte et download");

        // Igen, og denne gang "Ja, fortsæt".
        await ClickExportAsync(page, user, () => AuditLogPage.ThresholdConfirm(page).IsVisibleAsync());
        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToBeVisibleAsync();
        await AuditLogPage.ThresholdConfirmButton(page).ClickAsync();

        var url = await AuditLogExport.WaitForNewUrlAsync(page, knownCount: 0);
        var response = await AuditLogExport.FetchAsync(page, url, FetchTimeoutMs);

        response.Status.Should().Be(200);
        var rows = AuditLogExport.ParseCsv(response.Text);
        rows.Should().HaveCount(Rows50001 + 1, "overskrift + 50.001 rækker");
        rows.Skip(1).Select(r => r[2]).Distinct().Should().Equal(data.Email);
    }

    // --- 250.000: bekræftelse, ikke fejl ---

    [Fact]
    public async Task Eksport_250000Raekker_ViserBekraeftelseOgIkkeFejl()
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        await SearchAsync(page, data, data.Day(0), data.Day(2), Rows250000);

        await ClickExportAsync(page, user, () => AuditLogPage.ThresholdConfirm(page).IsVisibleAsync());

        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page))
            .ToContainTextAsync("Eksporten omfatter 250000 rækker og kan tage et øjeblik. Fortsæt?");
        await Assertions.Expect(AuditLogPage.ExportError(page)).ToHaveCountAsync(0);
        (await AuditLogExport.OpenedUrlsAsync(page)).Should().BeEmpty();

        await AuditLogPage.ThresholdCancelButton(page).ClickAsync();
        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToHaveCountAsync(0);
    }

    // --- 250.001: afvisning ---

    [Fact]
    public async Task Eksport_250001Raekker_AfvisesMedFejlUdenBekraeftelseOgDownload()
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        await SearchAsync(page, data, from: null, to: null, Rows250001);

        await ClickExportAsync(page, user, () => AuditLogPage.ExportError(page).IsVisibleAsync());

        await Assertions.Expect(AuditLogPage.ExportError(page)).ToContainTextAsync(
            "Error: Eksporten omfatter 250001 rækker, hvilket overstiger grænsen på 250000. " +
            "Indsnævr filteret og prøv igen.");
        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToHaveCountAsync(0);

        var opened = await BlazorInteraction.WaitUntilAsync(
            async () => (await AuditLogExport.OpenedUrlsAsync(page)).Count > 0, TimeSpan.FromSeconds(1));
        opened.Should().BeFalse("en afvist eksport må ikke starte et download");
    }

    [Fact]
    public async Task Endpoint_250001Raekker_GiverUbehandlelig422MedForklaring()
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        // Et gyldigt token fås ved at eksportere 50.000 rækker. Derefter fjernes datofiltrene fra URL'en, så
        // filteret rammer alle 250.001 rækker. UI'et ville aldrig udstede en sådan URL.
        await SearchAsync(page, data, data.Day(0), data.Day(0), Rows50000);
        var url = await ExportAsync(page, user);

        var uri = new Uri(url);
        var query = HttpUtility.ParseQueryString(uri.Query);
        query.Remove("fromUtc");
        query.Remove("toUtc");
        var widened = $"{uri.GetLeftPart(UriPartial.Path)}?{query}";

        var response = await AuditLogExport.FetchAsync(page, widened, FetchTimeoutMs);

        response.Status.Should().Be(422);
        response.FileName.Should().BeNull();

        using var problem = JsonDocument.Parse(response.Body);
        var detail = problem.RootElement.GetProperty("detail").GetString();
        detail.Should().Contain("250001 rækker").And.Contain("grænse på 250000");
    }

    // --- Baggrundseksport over grænsen ---

    [Fact]
    public async Task Baggrundseksport_250001Raekker_EnderSomFejletMedFejltekst()
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        await SearchAsync(page, data, from: null, to: null, Rows250001);

        // Oprettelsen tjekker ikke rækkeantallet: jobbet afvises først, når baggrundstjenesten behandler det.
        await ClickWithStepUpAsync(
            page, user, AuditLogPage.BackgroundExportButton(page), () => AuditLogPage.ExportSuccess(page).IsVisibleAsync());

        await Assertions.Expect(AuditLogPage.ExportSuccess(page)).ToContainTextAsync("Eksporten er sat i kø");
        await Assertions.Expect(AuditLogPage.JobRows(page)).ToHaveCountAsync(1);

        var row = AuditLogPage.JobRows(page).Nth(0);
        var refresh = page.Locator(".audit-log-export-jobs-panel")
            .GetByRole(AriaRole.Button, new() { Name = "Opdater", Exact = true });

        // Baggrundstjenesten poller hvert 10. sekund. Listen genindlæses med "Opdater".
        string? status = null;
        var failed = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            await refresh.ClickAsync();

            try
            {
                status = (await row.Locator(".badge-role").InnerTextAsync(new() { Timeout = 2000 })).Trim();
            }
            catch (PlaywrightException)
            {
                status = null; // listen er midlertidigt væk under genindlæsning
            }

            if (status == "Fejlet")
            {
                return true;
            }

            await Task.Delay(JobPoll, TestContext.Current.CancellationToken);
            return false;
        }, JobTimeout);

        failed.Should().BeTrue($"jobbet skulle ende som 'Fejlet' inden for {JobTimeout}, men status var '{status}'");

        await Assertions.Expect(AuditLogPage.Cell(row, 1)).ToHaveTextAsync("Csv");
        await Assertions.Expect(row.Locator(".meta")).ToContainTextAsync("Eksporten omfatter 250001 rækker");
        await Assertions.Expect(row.Locator(".meta")).ToContainTextAsync("grænsen på 250000");
        await Assertions.Expect(row.GetByRole(AriaRole.Link, new() { Name = "Download", Exact = true }))
            .ToHaveCountAsync(0);
    }

    // --- Skærmbilleder ---

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Skaermbilleder_BekraeftelseOgGraense(int width)
    {
        var data = await EnsureDatasetAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, width);

        await SearchAsync(page, data, data.Day(0), data.Day(1), Rows50001);
        await ClickExportAsync(page, user, () => AuditLogPage.ThresholdConfirm(page).IsVisibleAsync());
        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToBeVisibleAsync();
        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J4_Bekraeftelse");

        await AuditLogPage.ThresholdCancelButton(page).ClickAsync();
        await Assertions.Expect(AuditLogPage.ThresholdConfirm(page)).ToHaveCountAsync(0);

        await SearchAsync(page, data, from: null, to: null, Rows250001);
        await ClickExportAsync(page, user, () => AuditLogPage.ExportError(page).IsVisibleAsync());
        await Assertions.Expect(AuditLogPage.ExportError(page)).ToContainTextAsync("overstiger grænsen på 250000");
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J4_Graense");
    }

    // --- Datasæt ---

    private Task<Dataset> EnsureDatasetAsync() =>
        Datasets.GetOrAdd(
            fixture.RootUri,
            _ => new Lazy<Task<Dataset>>(() => SeedDatasetAsync(fixture.AppConnectionString))).Value;

    private static async Task<Dataset> SeedDatasetAsync(string connectionString)
    {
        var data = new Dataset(
            AuditLogTestData.NewUserEmail(),
            AuditLogTestData.NewEntityName("J4v"),
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-60));

        (int DayOffset, int Count)[] groups = [(0, 50_000), (1, 1), (2, 199_999), (3, 1)];

        foreach (var (dayOffset, count) in groups)
        {
            var noon = new DateTimeOffset(
                DateTime.SpecifyKind(data.Day(dayOffset).ToDateTime(new TimeOnly(12, 0)), DateTimeKind.Utc));

            await AuditLogTestData.SeedBulkAsync(connectionString, new AuditLogTestData.BulkSeed(
                data.Entity, data.Email, count, AuditAction.Update, noon, StepSeconds: 0));
        }

        return data;
    }

    // --- Hjælpere ---

    private async Task<SmokeTestUser> NewAuditorAsync() => await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);

    /// <summary>Søger på datasættets e-mail (+ evt. datoer) og venter, til totalen stemmer (tabellen læser op til 250.001 rækker).</summary>
    private static async Task SearchAsync(IPage page, Dataset data, DateOnly? from, DateOnly? to, int expectedTotal)
    {
        await AuditLogPage.SetTextAsync(AuditLogPage.UserEmailInput(page), data.Email);
        await AuditLogPage.SetDateAsync(AuditLogPage.FromInput(page), from);
        await AuditLogPage.SetDateAsync(AuditLogPage.ToInput(page), to);
        await AuditLogPage.SearchAsync(page);

        await Assertions.Expect(AuditLogPage.PaginationInfo(page))
            .ToContainTextAsync($"({expectedTotal} rækker i alt)", new() { Timeout = SearchTimeoutMs });
    }

    /// <summary>
    /// Klikker "Eksportér" og gennemfører step-up, hvis modalen vises. Returnerer den opsnappede URL.
    /// Bruges kun, når eksporten ikke kræver bekræftelse.
    /// </summary>
    private static async Task<string> ExportAsync(IPage page, SmokeTestUser user)
    {
        var known = (await AuditLogExport.OpenedUrlsAsync(page)).Count;

        await ClickExportAsync(page, user, async () => (await AuditLogExport.OpenedUrlsAsync(page)).Count > known);

        return await AuditLogExport.WaitForNewUrlAsync(page, known);
    }

    private static Task ClickExportAsync(IPage page, SmokeTestUser user, Func<Task<bool>> outcomeWithoutModal) =>
        ClickWithStepUpAsync(page, user, AuditLogPage.ExportButton(page), outcomeWithoutModal);

    /// <summary>
    /// Klikker og gennemfører step-up, hvis modalen vises (kun første gang inden for re-auth-vinduet i
    /// circuiten). Handlingen fortsætter af sig selv efter modalen. Kalderen afventer selv udfaldet.
    /// Dublerer ExportAsync i AuditLogEksportTests (kandidat til dedup).
    /// </summary>
    private static async Task ClickWithStepUpAsync(
        IPage page, SmokeTestUser user, ILocator button, Func<Task<bool>> outcomeWithoutModal)
    {
        await button.ClickAsync();

        await BlazorInteraction.WaitUntilAsync(
            async () => await StepUpModal.Dialog(page).IsVisibleAsync() || await outcomeWithoutModal(),
            TimeSpan.FromSeconds(30));

        if (await StepUpModal.Dialog(page).IsVisibleAsync())
        {
            await StepUpModal.CompleteAsync(page, user);
        }
    }
}
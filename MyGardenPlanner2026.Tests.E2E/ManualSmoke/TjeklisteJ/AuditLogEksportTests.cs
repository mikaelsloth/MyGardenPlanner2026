namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteJ;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.Json;
using System.Text.RegularExpressions;

/// <summary>
/// Tjekliste J4 (lille datasæt): eksport på /admin/audit-log. Hver test seeder 3 rækker med unik e-mail
/// (nyeste først: Delete, Update, Create), filtrerer på e-mailen og bruger en frisk AuditViewer.
/// Eksporten udløses med window.open: AuditLogExport opsnapper URL'en, og testen henter den med browserens
/// cookies. Det verificerer step-up, parametre, status, filnavn og indhold, men ikke selve browser-downloadet.
/// Grænserne 50.000/250.000 og baggrundseksport over grænsen ligger i PR 8 (egen collection).
/// Skærmbilleder: Auditlog_J4_StepUp (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed partial class AuditLogEksportTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste J";
    private const string TestName = "Auditlog";

    private const string ValueA = "{\"Name\":\"A\",\"Tags\":\"x,y\"}";
    private const string ValueB = "{\"Name\":\"B\",\"Tags\":\"x,y\"}";

    private static readonly string[] CsvHeader =
        ["Id", "UserId", "UserEmail", "IpAddress", "Action", "EntityName", "EntityId", "OldValues", "NewValues", "TimestampUtc"];

    private enum RowKind
    {
        Create,
        Update,
        Delete,
    }

    private sealed record Seed(string Entity, string Prefix, string UserId, string Email, DateTimeOffset Base)
    {
        public string Id(RowKind kind) => $"{Prefix}-{kind.ToString().ToLowerInvariant()}";

        public DateTimeOffset TimestampOf(RowKind kind) => Base.AddMinutes((int)kind);

        /// <summary>Eksporten sorteres nyeste først.</summary>
        public string[] IdsNewestFirst() => [Id(RowKind.Delete), Id(RowKind.Update), Id(RowKind.Create)];
    }

    // --- a. Step-up ---

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Eksport_FriskCircuit_KraeverStepUpOgAnnullerStarterIntet(int width)
    {
        var seed = await SeedAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, width);
        await FilterByEmailAsync(page, seed);

        // Første handling i en frisk circuit: modalen vises, og intet download startes.
        await StepUpModal.OpenAsync(page, () => AuditLogPage.ExportButton(page).ClickAsync());
        await Assertions.Expect(StepUpModal.PasswordField(page)).ToBeVisibleAsync();
        await Assertions.Expect(StepUpModal.TotpField(page)).ToBeVisibleAsync();
        (await AuditLogExport.OpenedUrlsAsync(page)).Should().BeEmpty();

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J4_StepUp");

        // Annullér: intet sker. Negativ kontrol: vent, til et download ville være startet.
        await StepUpModal.CancelButton(page).ClickAsync();
        await Assertions.Expect(StepUpModal.Dialog(page)).ToBeHiddenAsync();

        var opened = await BlazorInteraction.WaitUntilAsync(
            async () => (await AuditLogExport.OpenedUrlsAsync(page)).Count > 0, TimeSpan.FromSeconds(1));
        opened.Should().BeFalse("Annullér må ikke starte et download");

        // Igen: der er stadig ikke gennemført step-up, så modalen vises igen. Korrekt step-up starter download.
        await StepUpModal.OpenAsync(page, () => AuditLogPage.ExportButton(page).ClickAsync());
        await StepUpModal.CompleteAsync(page, user);

        var url = await AuditLogExport.WaitForNewUrlAsync(page, knownCount: 0);
        var response = await AuditLogExport.FetchAsync(page, url);

        response.Status.Should().Be(200);
        ExpectFileName(response, AuditLogExportFormat.Csv);
    }

    // --- b. Ugyldigt token ---

    [Fact]
    public async Task UgyldigtToken_GiverForbudt403MedForklaring()
    {
        var seed = await SeedAsync();
        var page = await AuditLogPage.OpenAsync(fixture, await NewAuditorAsync(), Viewports.Desktop);

        var response = await AuditLogExport.FetchWithInvalidTokenAsync(page, fixture.RootUri);

        response.Status.Should().Be(403);
        response.FileName.Should().BeNull();

        // Problem details skriver ikke-ASCII som \u-escapes i den rå tekst: parse JSON i stedet.
        using var problem = JsonDocument.Parse(response.Body);
        problem.RootElement.GetProperty("detail").GetString()
            .Should().Contain("Eksport-token er ugyldig eller udløbet");

        seed.Should().NotBeNull(); // seed sikrer mindst én række, så siden kan åbnes interaktivt
    }

    // --- c. Formater ---

    [Theory]
    [InlineData(AuditLogExportFormat.Csv)]
    [InlineData(AuditLogExportFormat.Json)]
    [InlineData(AuditLogExportFormat.Xlsx)]
    public async Task Eksport_MedFilter_GiverFilMedKunDeSoegteRaekker(AuditLogExportFormat format)
    {
        var seed = await SeedAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);
        await FilterByEmailAsync(page, seed);

        var url = await ExportAsync(page, user, format);
        var response = await AuditLogExport.FetchAsync(page, url);

        url.Should().Contain($"format={format}").And.Contain($"userEmail={Uri.EscapeDataString(seed.Email)}");
        response.Status.Should().Be(200);
        ExpectFileName(response, format);

        switch (format)
        {
            case AuditLogExportFormat.Csv:
                ExpectCsv(response, seed);
                break;
            case AuditLogExportFormat.Json:
                ExpectJson(response, seed);
                break;
            default:
                ExpectXlsx(response, seed);
                break;
        }
    }

    [Fact]
    public async Task Eksport_BrugerSidstSoegteFilter_IkkeUsoegteFelter()
    {
        var seed = await SeedAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);
        await FilterByEmailAsync(page, seed);

        // Ændret men ikke søgt: må ikke påvirke eksporten.
        await AuditLogPage.SetTextAsync(AuditLogPage.UserEmailInput(page), AuditLogTestData.NewUserEmail());

        var url = await ExportAsync(page, user, AuditLogExportFormat.Csv);
        var response = await AuditLogExport.FetchAsync(page, url);

        url.Should().Contain($"userEmail={Uri.EscapeDataString(seed.Email)}");
        AuditLogExport.ParseCsv(response.Text).Should().HaveCount(4, "overskrift + de 3 søgte rækker");
    }

    // --- e. Baggrundseksport ---

    [Fact]
    public async Task Baggrundseksport_SaettesIKoeOgStaarIJobListen()
    {
        var seed = await SeedAsync();
        var user = await NewAuditorAsync();
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);
        await FilterByEmailAsync(page, seed);

        await StepUpModal.OpenAsync(page, () => AuditLogPage.BackgroundExportButton(page).ClickAsync());
        await StepUpModal.CompleteAsync(page, user);

        await Assertions.Expect(AuditLogPage.ExportSuccess(page)).ToContainTextAsync("Eksporten er sat i kø");

        // Frisk bruger: præcis ét job. Status afhænger af, hvor hurtigt baggrundstjenesten (10 sek.) når det.
        await Assertions.Expect(AuditLogPage.JobRows(page)).ToHaveCountAsync(1);
        var row = AuditLogPage.JobRows(page).Nth(0);

        await Assertions.Expect(AuditLogPage.Cell(row, 1)).ToHaveTextAsync("Csv");
        await Assertions.Expect(row.Locator(".badge-role"))
            .ToHaveTextAsync(StatusTexts());
    }

    // --- Verifikation af filer ---

    private static void ExpectCsv(ExportResponse response, Seed seed)
    {
        response.ContentType.Should().StartWith("text/csv");

        var rows = AuditLogExport.ParseCsv(response.Text);

        rows.Should().HaveCount(4, "overskrift + 3 rækker");
        rows[0].Should().Equal(CsvHeader);
        rows.Skip(1).Select(r => r[6]).Should().Equal(seed.IdsNewestFirst());
        rows.Skip(1).Select(r => r[2]).Should().OnlyContain(e => e == seed.Email);
        rows.Skip(1).Select(r => r[4]).Should().Equal("Delete", "Update", "Create");

        // Komma og anførselstegn i JSON skal overleve CSV-escaping.
        var update = rows[2];
        update[7].Should().Be(ValueA);
        update[8].Should().Be(ValueB);
    }

    private static void ExpectJson(ExportResponse response, Seed seed)
    {
        response.ContentType.Should().StartWith("application/json");
        AuditLogExport.JsonRowCount(response.Body).Should().Be(3);

        using var document = JsonDocument.Parse(response.Body);
        var items = document.RootElement.EnumerateArray().ToList();

        items.Select(i => i.GetProperty("EntityId").GetString()).Should().Equal(seed.IdsNewestFirst());
        items.Select(i => i.GetProperty("UserEmail").GetString()).Should().OnlyContain(e => e == seed.Email);
        items.Select(i => i.GetProperty("Action").GetString()).Should().Equal("Delete", "Update", "Create");

        items[1].GetProperty("OldValues").GetString().Should().Be(ValueA);
        items[1].GetProperty("NewValues").GetString().Should().Be(ValueB);
    }

    private static void ExpectXlsx(ExportResponse response, Seed seed)
    {
        response.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        AuditLogExport.IsZip(response.Body).Should().BeTrue();
        AuditLogExport.XlsxRowCount(response.Body).Should().Be(3);

        AuditLogExport.XlsxColumn(response.Body, 7).Should().Equal(seed.IdsNewestFirst());
        AuditLogExport.XlsxColumn(response.Body, 3).Should().OnlyContain(e => e == seed.Email);
    }

    private static void ExpectFileName(ExportResponse response, AuditLogExportFormat format)
    {
        var extension = format switch
        {
            AuditLogExportFormat.Csv => "csv",
            AuditLogExportFormat.Json => "json",
            AuditLogExportFormat.Xlsx => "xlsx",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Ukendt eksportformat."),
        };

        response.FileName.Should().MatchRegex($@"^audit-log-export-\d{{8}}-\d{{6}}\.{extension}$");
    }

    // --- Hjælpere ---

    private async Task<SmokeTestUser> NewAuditorAsync() => await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);

    /// <summary>Seeder før siden åbnes: dropdownen og første række hentes ved indlæsning.</summary>
    private async Task<Seed> SeedAsync()
    {
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var seed = new Seed(
            AuditLogTestData.NewEntityName("J4"),
            prefix,
            $"j4-{prefix}",
            AuditLogTestData.NewUserEmail(),
            new DateTimeOffset(DateTime.UtcNow.Date.AddDays(-30).AddHours(10), TimeSpan.Zero));

        AuditLog Row(RowKind kind, AuditAction action, string? oldValues, string? newValues) =>
            AuditLogTestData.Row(
                seed.Entity, seed.Id(kind), action, seed.TimestampOf(kind),
                userId: seed.UserId, userEmail: seed.Email, ipAddress: "10.0.0.7",
                oldValues: oldValues, newValues: newValues);

        await AuditLogTestData.SeedAsync(fixture.AppConnectionString,
        [
            Row(RowKind.Create, AuditAction.Create, null, ValueA),
            Row(RowKind.Update, AuditAction.Update, ValueA, ValueB),
            Row(RowKind.Delete, AuditAction.Delete, ValueB, null),
        ]);

        return seed;
    }

    /// <summary>Søger på den unikke e-mail og venter, til præcis de 3 seedede rækker vises.</summary>
    private static async Task FilterByEmailAsync(IPage page, Seed seed)
    {
        await AuditLogPage.SetTextAsync(AuditLogPage.UserEmailInput(page), seed.Email);
        await AuditLogPage.SearchAsync(page);

        await Assertions.Expect(AuditLogPage.Rows(page)).ToHaveCountAsync(3);
        await Assertions.Expect(AuditLogPage.Rows(page).Filter(new() { HasText = seed.Email })).ToHaveCountAsync(3);
    }

    /// <summary>
    /// Vælger format, klikker Eksportér og gennemfører step-up, hvis modalen vises (kun første gang inden for
    /// re-auth-vinduet i circuiten). Returnerer den opsnappede eksport-URL.
    /// </summary>
    private static async Task<string> ExportAsync(IPage page, SmokeTestUser user, AuditLogExportFormat format)
    {
        var known = (await AuditLogExport.OpenedUrlsAsync(page)).Count;

        await AuditLogPage.SelectExportFormatAsync(page, format);
        await AuditLogPage.ExportButton(page).ClickAsync();

        await BlazorInteraction.WaitUntilAsync(
            async () => await StepUpModal.Dialog(page).IsVisibleAsync()
                || (await AuditLogExport.OpenedUrlsAsync(page)).Count > known,
            TimeSpan.FromSeconds(10));

        if (await StepUpModal.Dialog(page).IsVisibleAsync())
        {
            await StepUpModal.CompleteAsync(page, user);
        }

        return await AuditLogExport.WaitForNewUrlAsync(page, known);
    }

    [GeneratedRegex("^(Afventer|Kører|Klar)$")]
    private static partial Regex StatusTexts();
}
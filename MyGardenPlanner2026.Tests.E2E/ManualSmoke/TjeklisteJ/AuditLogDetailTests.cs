namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteJ;

using FluentAssertions;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Globalization;
using System.Text.Json;

/// <summary>
/// Tjekliste J3: detaljemodalen på /admin/audit-log. Hver test seeder 5 rækker med en unik entitet
/// (create: Old null | update: kompakt JSON | delete: New null | invalid: ugyldig JSON | empty: tomme strenge),
/// filtrerer på entiteten og bruger en frisk AuditViewer. Forventet JSON-formatering er hardcodet.
/// Layout: to kolonner fra 940 px, under hinanden under 940 px. Skærmbilleder: Auditlog_J3_Detaljer (&lt;bredde&gt;).
/// En rigtig række (J2b) dækkes af J2.
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AuditLogDetailTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste J";
    private const string TestName = "Auditlog";

    private const string NoData = "(ingen data)";

    private const string UpdateOld = "{\"Name\":\"Gammel\",\"Value\":1}";
    private const string UpdateNew = "{\"Name\":\"Ny\",\"Child\":{\"Ok\":true}}";
    private const string UpdateOldPretty = "{\n  \"Name\": \"Gammel\",\n  \"Value\": 1\n}";
    private const string UpdateNewPretty = "{\n  \"Name\": \"Ny\",\n  \"Child\": {\n    \"Ok\": true\n  }\n}";

    private const string FullRow = "{\"Id\":\"abc\",\"IsDeleted\":false}";
    private const string FullRowPretty = "{\n  \"Id\": \"abc\",\n  \"IsDeleted\": false\n}";

    private const string InvalidOld = "{ikke json";
    private const string InvalidNew = "[1,2";

    private static readonly CultureInfo DanishCulture = new("da-DK");

    private enum RowKind
    {
        Create,
        Update,
        Delete,
        Invalid,
        Empty,
    }

    private sealed record Seed(
        string Entity, string Prefix, string UserId, string Email, string Ip, DateTimeOffset Base)
    {
        public string Id(RowKind kind) => $"{Prefix}-{kind.ToString().ToLowerInvariant()}";

        public DateTimeOffset TimestampOf(RowKind kind) => Base.AddMinutes((int)kind);
    }

    public static TheoryData<int> Widths()
    {
        var data = new TheoryData<int>();
        foreach (var width in Viewports.All)
        {
            data.Add(width);
        }

        return data;
    }

    // --- Titel og metalinje ---

    [Fact]
    public async Task Modal_ViserRolleDialogTitelMetalinjeOgOverskrifter()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);

        await OpenDetailsAsync(page, seed, RowKind.Update);

        var title = $"Detaljer — {seed.Entity} ({seed.Id(RowKind.Update)})";
        await Assertions.Expect(page.GetByRole(AriaRole.Dialog, new() { Name = title })).ToBeVisibleAsync();

        var meta = AuditLogPage.DetailDialog(page).Locator("p.meta");
        await Assertions.Expect(meta).ToContainTextAsync(
            seed.TimestampOf(RowKind.Update).ToLocalTime().ToString("g", DanishCulture));
        await Assertions.Expect(meta).ToContainTextAsync(seed.Email);
        await Assertions.Expect(meta).ToContainTextAsync($"({seed.UserId})");
        await Assertions.Expect(meta).ToContainTextAsync(seed.Ip);

        var dialog = AuditLogPage.DetailDialog(page);
        await Assertions.Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Tidligere værdier" })).ToBeVisibleAsync();
        await Assertions.Expect(dialog.GetByRole(AriaRole.Heading, new() { Name = "Nye værdier" })).ToBeVisibleAsync();
    }

    // --- JSON ---

    [Fact]
    public async Task Update_KompaktJson_VisesIndrykketOgFlerlinjetOgKanParses()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);

        await OpenDetailsAsync(page, seed, RowKind.Update);

        var oldText = await ReadBlockAsync(page, 0);
        var newText = await ReadBlockAsync(page, 1);

        oldText.Should().Be(UpdateOldPretty);
        newText.Should().Be(UpdateNewPretty);
        oldText.Split('\n').Length.Should().BeGreaterThan(1);

        var parseOld = () => JsonDocument.Parse(oldText);
        var parseNew = () => JsonDocument.Parse(newText);
        parseOld.Should().NotThrow();
        parseNew.Should().NotThrow();
    }

    [Fact]
    public async Task Create_OldNull_ViserIngenDataOgNewFormateret()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);

        await OpenDetailsAsync(page, seed, RowKind.Create);

        (await ReadBlockAsync(page, 0)).Should().Be(NoData);
        (await ReadBlockAsync(page, 1)).Should().Be(FullRowPretty);
    }

    [Fact]
    public async Task Delete_NewNull_ViserOldFormateretOgIngenDataIsaerNew()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);

        await OpenDetailsAsync(page, seed, RowKind.Delete);

        (await ReadBlockAsync(page, 0)).Should().Be(FullRowPretty);
        (await ReadBlockAsync(page, 1)).Should().Be(NoData);
    }

    [Fact]
    public async Task TommeStrenge_ViserIngenData()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);

        await OpenDetailsAsync(page, seed, RowKind.Empty);

        (await ReadBlockAsync(page, 0)).Should().Be(NoData);
        (await ReadBlockAsync(page, 1)).Should().Be(NoData);
    }

    [Fact]
    public async Task UgyldigJson_ViserSomRaaTekstUdenFejl()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);

        await OpenDetailsAsync(page, seed, RowKind.Invalid);

        (await ReadBlockAsync(page, 0)).Should().Be(InvalidOld);
        (await ReadBlockAsync(page, 1)).Should().Be(InvalidNew);
        await Assertions.Expect(page.Locator("#blazor-error-ui")).ToBeHiddenAsync();
        await Assertions.Expect(AuditLogPage.DetailDialog(page)).ToBeVisibleAsync();
    }

    // --- Layout ---

    [Theory]
    [MemberData(nameof(Widths))]
    public async Task Layout_KolonnerStaarSideOmSideFra940PxOgUnderHinandenUnder(int width)
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, width);

        await OpenDetailsAsync(page, seed, RowKind.Update);

        var columns = AuditLogPage.DetailDialog(page).Locator(".audit-log-detail-column");
        await Assertions.Expect(columns).ToHaveCountAsync(2);

        var first = await columns.Nth(0).BoundingBoxAsync()
            ?? throw new InvalidOperationException("Første kolonne har ingen bounding box.");
        var second = await columns.Nth(1).BoundingBoxAsync()
            ?? throw new InvalidOperationException("Anden kolonne har ingen bounding box.");

        if (width >= Viewports.AtBreakpoint)
        {
            ((double)second.X).Should().BeGreaterThanOrEqualTo(first.X + first.Width - 1, "kolonnerne skal stå side om side ved {0} px", width);
            Math.Abs(second.Y - first.Y).Should().BeLessThan(2, "kolonnerne skal starte i samme højde ved {0} px", width);
        }
        else
        {
            ((double)second.Y).Should().BeGreaterThanOrEqualTo(first.Y + first.Height - 1, "kolonnerne skal stå under hinanden ved {0} px", width);
            Math.Abs(second.X - first.X).Should().BeLessThan(2, "kolonnerne skal flugte ved {0} px", width);
        }

        // "Luk" skal kunne nås (dialogen scroller internt på lave skærme).
        await AuditLogPage.CloseButton(page).ClickAsync();
        await Assertions.Expect(AuditLogPage.DetailDialog(page)).ToBeHiddenAsync();
    }

    // --- Lukning ---

    [Fact]
    public async Task Luk_OgKlikPaaBaggrund_LukkerMensKlikIDialogenIkkeGoer()
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, Viewports.Desktop);
        var dialog = AuditLogPage.DetailDialog(page);

        // Klik inde i dialogen lukker ikke. Negativ kontrol: vent, til en evt. lukning ville være sket.
        await OpenDetailsAsync(page, seed, RowKind.Update);
        await dialog.Locator("h2").ClickAsync();

        var closed = await BlazorInteraction.WaitUntilAsync(
            async () => !await dialog.IsVisibleAsync(), TimeSpan.FromSeconds(1));
        closed.Should().BeFalse("klik inde i dialogen må ikke lukke den");

        // "Luk".
        await AuditLogPage.CloseButton(page).ClickAsync();
        await Assertions.Expect(dialog).ToBeHiddenAsync();

        // Klik på baggrunden (uden for dialogen, som højst er 900 px bred og centreret ved 1280 px).
        await OpenDetailsAsync(page, seed, RowKind.Create);
        await AuditLogPage.DetailBackdrop(page).ClickAsync(new() { Position = new Position { X = 10, Y = 10 } });
        await Assertions.Expect(dialog).ToBeHiddenAsync();
    }

    // --- Skærmbilleder ---

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Skaermbilleder_Detaljer(int width)
    {
        var seed = await SeedAsync();
        var page = await OpenFilteredAsync(seed, width);

        await OpenDetailsAsync(page, seed, RowKind.Update);
        await Assertions.Expect(AuditLogPage.JsonBlock(page, 1)).ToContainTextAsync("Child");
        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J3_Detaljer");
    }

    // --- Hjælpere ---

    /// <summary>Seeder før siden åbnes: dropdownen og første række hentes ved indlæsning.</summary>
    private async Task<Seed> SeedAsync()
    {
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var seed = new Seed(
            AuditLogTestData.NewEntityName("J3"),
            prefix,
            $"j3-{prefix}",
            AuditLogTestData.NewUserEmail(),
            "10.0.0.7",
            new DateTimeOffset(DateTime.UtcNow.Date.AddDays(-30).AddHours(10), TimeSpan.Zero));

        AuditLog Row(RowKind kind, AuditAction action, string? oldValues, string? newValues) =>
            AuditLogTestData.Row(
                seed.Entity, seed.Id(kind), action, seed.TimestampOf(kind),
                userId: seed.UserId, userEmail: seed.Email, ipAddress: seed.Ip,
                oldValues: oldValues, newValues: newValues);

        await AuditLogTestData.SeedAsync(fixture.AppConnectionString,
        [
            Row(RowKind.Create, AuditAction.Create, null, FullRow),
            Row(RowKind.Update, AuditAction.Update, UpdateOld, UpdateNew),
            Row(RowKind.Delete, AuditAction.Delete, FullRow, null),
            Row(RowKind.Invalid, AuditAction.Update, InvalidOld, InvalidNew),
            Row(RowKind.Empty, AuditAction.Update, string.Empty, string.Empty),
        ]);

        return seed;
    }

    /// <summary>Åbner siden med en frisk AuditViewer og filtrerer på den seedede entitet (præcis 5 rækker).</summary>
    private async Task<IPage> OpenFilteredAsync(Seed seed, int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var page = await AuditLogPage.OpenAsync(fixture, user, width);

        await AuditLogPage.SelectEntityAsync(page, seed.Entity);
        await AuditLogPage.SearchAsync(page);

        await Assertions.Expect(AuditLogPage.Rows(page)).ToHaveCountAsync(5);
        await Assertions.Expect(AuditLogPage.RowsFor(page, seed.Entity)).ToHaveCountAsync(5);

        return page;
    }

    private static async Task OpenDetailsAsync(IPage page, Seed seed, RowKind kind)
    {
        var row = AuditLogPage.Rows(page).Filter(new() { HasText = seed.Id(kind) });
        await Assertions.Expect(row).ToHaveCountAsync(1);

        await AuditLogPage.OpenDetailsAsync(row);
        await Assertions.Expect(AuditLogPage.DetailDialog(page)).ToBeVisibleAsync();
    }

    /// <summary>
    /// Rå tekst med linjeskift normaliseret til \n. ToHaveTextAsync normaliserer whitespace og kan derfor
    /// ikke bruges til at verificere indrykning og flerlinjet formatering.
    /// </summary>
    private static async Task<string> ReadBlockAsync(IPage page, int index) =>
        ((await AuditLogPage.JsonBlock(page, index).TextContentAsync()) ?? string.Empty).Replace("\r\n", "\n");
}
namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteJ;

using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Tests.E2E.Screenshots;

/// <summary>
/// Tjekliste J1: filtrering, sortering, paginering og gemt præference på /admin/audit-log. Hver test seeder
/// 16 rækker med unikke markører (entitet A: 12 rækker, entitet B: 4) og bruger en frisk AuditViewer, da
/// visningspræferencer er pr. bruger. Anker D = i dag (UTC) - 30 dage. Datoer er UTC; "Til" er inklusiv hele dagen.
///
/// Seed (EntityId = "&lt;prefix&gt;-A01" osv.; X/Y = bruger; tid = dagoffset fra D):
///   A01 Create X D-1 23:59:59 | A02 Create X D 00:00:00 | A03 Update X D 12:00:00 | A04 Update Y D 23:59:59
///   A05 Delete Y D+1 00:00:00 | A06 Create Y D+2 10:00 | A07 Update X D+3 10:00 | A08 Update Y D+4 10:00
///   A09 Delete X D+5 10:00 | A10 Delete Y D+6 10:00 | A11 Create X D+7 10:00 | A12 Update X D+8 10:00
///   B1 Create X D 13:00 | B2 Update Y D+1 13:00 | B3 Delete X D+2 13:00 | B4 Update X D+3 13:00
/// Forventede lister er hardcodede (nyeste først). Skærmbilleder: Auditlog_J1_Filtreret og
/// Auditlog_J1_IngenResultater (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AuditLogFilterTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste J";
    private const string TestName = "Auditlog";

    private sealed record Seed(
        string Prefix,
        string EntityA,
        string EntityB,
        string UserIdX,
        string EmailX,
        string UserIdY,
        string EmailY,
        DateOnly Day)
    {
        public string[] AIds(params int[] numbers) => [.. numbers.Select(n => $"{Prefix}-A{n:00}")];

        public string[] BIds(params int[] numbers) => [.. numbers.Select(n => $"{Prefix}-B{n:00}")];
    }

    private sealed record SearchSpec(
        string? Entity = null,
        string? EntityId = null,
        string? UserId = null,
        string? Email = null,
        AuditAction? Action = null,
        DateOnly? From = null,
        DateOnly? To = null,
        int? PageSize = null);

    // --- Dropdown og entitet ---

    [Fact]
    public async Task Dropdown_ViserAlleOgSeededeEntiteterAlfabetisk()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        var options = (await AuditLogPage.EntitySelect(page).Locator("option").AllInnerTextsAsync())
            .Select(o => o.Trim()).ToList();

        options[0].Should().Be("Alle");
        options.Should().Contain([seed.EntityA, seed.EntityB]);
        options.IndexOf(seed.EntityA).Should().BeLessThan(options.IndexOf(seed.EntityB));
    }

    [Fact]
    public async Task EntitetFilter_ViserKunValgtEntitetsRaekker()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA));
        await ExpectRowsAsync(page, seed.AIds(12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1));

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityB));
        await ExpectRowsAsync(page, seed.BIds(4, 3, 2, 1));
    }

    // --- Eksakt match ---

    [Fact]
    public async Task EntitetsIdBrugerIdOgEmail_MatcherEksaktOgDelstrengGiverIngenResultater()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        // Bruger X har 10 rækker (7 i A, 3 i B), nyeste først.
        var userXRows = new[] { seed.AIds(12, 11, 9), seed.BIds(4), seed.AIds(7), seed.BIds(3, 1), seed.AIds(3, 2, 1) }
            .SelectMany(ids => ids).ToArray();

        await ApplyAsync(page, new SearchSpec(EntityId: seed.AIds(3)[0]));
        await ExpectRowsAsync(page, seed.AIds(3));

        await ApplyAsync(page, new SearchSpec(EntityId: $"{seed.Prefix}-A0"));
        await ExpectNoResultsAsync(page);

        await ApplyAsync(page, new SearchSpec(UserId: seed.UserIdX));
        await ExpectRowsAsync(page, userXRows);

        await ApplyAsync(page, new SearchSpec(UserId: seed.UserIdX[..^1]));
        await ExpectNoResultsAsync(page);

        await ApplyAsync(page, new SearchSpec(Email: seed.EmailX));
        await ExpectRowsAsync(page, userXRows);
        (await AuditLogPage.Rows(page).Locator("td:nth-child(2)").AllInnerTextsAsync())
            .Should().OnlyContain(e => e.Trim() == seed.EmailX);

        await ApplyAsync(page, new SearchSpec(Email: seed.EmailX[..^1]));
        await ExpectNoResultsAsync(page);
    }

    // --- Handling ---

    [Theory]
    [InlineData(AuditAction.Create, "Oprettet", new[] { 11, 6, 2, 1 })]
    [InlineData(AuditAction.Update, "Opdateret", new[] { 12, 8, 7, 4, 3 })]
    [InlineData(AuditAction.Delete, "Slettet", new[] { 10, 9, 5 })]
    public async Task Handling_ViserKunValgtTypeMedKorrektBadge(AuditAction action, string label, int[] expected)
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, Action: action));

        await ExpectRowsAsync(page, seed.AIds(expected));
        (await AuditLogPage.Rows(page).Locator("td:nth-child(3)").AllInnerTextsAsync())
            .Should().OnlyContain(l => l.Trim() == label);
    }

    // --- Kombination (AND) ---

    [Fact]
    public async Task KombineredeFelter_VirkerSomAnd()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, Email: seed.EmailX, Action: AuditAction.Update));
        await ExpectRowsAsync(page, seed.AIds(12, 7, 3));

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, UserId: seed.UserIdY, Action: AuditAction.Create));
        await ExpectRowsAsync(page, seed.AIds(6));

        // Modstridende bruger-ID og e-mail: ingen række opfylder begge.
        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, UserId: seed.UserIdX, Email: seed.EmailY));
        await ExpectNoResultsAsync(page);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, Email: seed.EmailX, Action: AuditAction.Delete));
        await ExpectRowsAsync(page, seed.AIds(9));

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityB, Email: seed.EmailX, Action: AuditAction.Update));
        await ExpectRowsAsync(page, seed.BIds(4));
    }

    // --- Datoer ---

    [Fact]
    public async Task FraOgTil_ErInklusiveHeleDagenOgFraStoerreEndTilGiverIngenResultater()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);
        var d = seed.Day;

        // Én dag: 00:00:00 og 23:59:59 er med. D-1 23:59:59 og D+1 00:00:00 (1 sek. udenfor) er ikke.
        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, From: d, To: d));
        await ExpectRowsAsync(page, seed.AIds(4, 3, 2));

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, From: d));
        await ExpectRowsAsync(page, seed.AIds(12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2));

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, To: d));
        await ExpectRowsAsync(page, seed.AIds(4, 3, 2, 1));

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, From: d.AddDays(1), To: d));
        await ExpectNoResultsAsync(page);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, From: d.AddDays(8), To: d.AddDays(8)));
        await ExpectRowsAsync(page, seed.AIds(12));
    }

    // --- Sortering og paginering ---

    [Fact]
    public async Task Sortering_ViserNyesteTidspunktFoerst()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA));

        await ExpectRowsAsync(page, seed.AIds(12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1));
    }

    [Fact]
    public async Task Paginering_ToSiderMedKorrekteKnapperOgAntal()
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(Viewports.Desktop);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, PageSize: 10));

        await ExpectRowsAsync(page, seed.AIds(12, 11, 10, 9, 8, 7, 6, 5, 4, 3));
        await Assertions.Expect(AuditLogPage.PaginationInfo(page)).ToHaveTextAsync("Side 1 af 2 (12 rækker i alt)");
        await Assertions.Expect(AuditLogPage.PreviousButton(page)).ToBeDisabledAsync();
        await Assertions.Expect(AuditLogPage.NextButton(page)).ToBeEnabledAsync();

        await AuditLogPage.NextButton(page).ClickAsync();

        await ExpectRowsAsync(page, seed.AIds(2, 1));
        await Assertions.Expect(AuditLogPage.PaginationInfo(page)).ToHaveTextAsync("Side 2 af 2 (12 rækker i alt)");
        await Assertions.Expect(AuditLogPage.NextButton(page)).ToBeDisabledAsync();
        await Assertions.Expect(AuditLogPage.PreviousButton(page)).ToBeEnabledAsync();

        await AuditLogPage.PreviousButton(page).ClickAsync();

        await ExpectRowsAsync(page, seed.AIds(12, 11, 10, 9, 8, 7, 6, 5, 4, 3));
        await Assertions.Expect(AuditLogPage.PaginationInfo(page)).ToHaveTextAsync("Side 1 af 2 (12 rækker i alt)");
    }

    // --- Gemt præference ---

    [Fact]
    public async Task Praeference_GenskabesEfterGenindlaesning_SidenavigationGemmesIkke_AndenBrugerSerStandard()
    {
        var seed = await SeedAsync();
        var user = await NewAuditorAsync();
        var userId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, user.Email);
        var page = await AuditLogPage.OpenAsync(fixture, user, Viewports.Desktop);

        // 11 rækker (A02..A12) med sidestørrelse 10 giver to sider.
        var firstPage = seed.AIds(12, 11, 10, 9, 8, 7, 6, 5, 4, 3);
        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, From: seed.Day, To: seed.Day.AddDays(8), PageSize: 10));
        await ExpectRowsAsync(page, firstPage);
        await WaitForSavedPreferenceAsync(userId, pageSize: 10);

        await AuditLogPage.ReloadAsync(page);

        await ExpectRowsAsync(page, firstPage);
        await Assertions.Expect(AuditLogPage.PaginationInfo(page)).ToHaveTextAsync("Side 1 af 2 (11 rækker i alt)");
        await ExpectSavedFieldsAsync(page, seed);

        // Sidenavigation gemmes ikke.
        await AuditLogPage.NextButton(page).ClickAsync();
        await ExpectRowsAsync(page, seed.AIds(2));
        await AuditLogPage.ReloadAsync(page);

        await ExpectRowsAsync(page, firstPage);
        await Assertions.Expect(AuditLogPage.PaginationInfo(page)).ToHaveTextAsync("Side 1 af 2 (11 rækker i alt)");

        // En anden bruger ser standard.
        var other = await AuditLogPage.OpenAsync(fixture, await NewAuditorAsync(), Viewports.Desktop);

        await Assertions.Expect(AuditLogPage.PageSizeSelect(other)).ToHaveValueAsync("25");
        await Assertions.Expect(AuditLogPage.EntitySelect(other)).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(AuditLogPage.FromInput(other)).ToHaveValueAsync(string.Empty);
        await Assertions.Expect(AuditLogPage.ToInput(other)).ToHaveValueAsync(string.Empty);
    }

    // --- Skærmbilleder ---

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Skaermbilleder_FiltreretOgIngenResultater(int width)
    {
        var seed = await SeedAsync();
        var page = await OpenAsync(width);

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, Email: seed.EmailX, Action: AuditAction.Update));
        await ExpectRowsAsync(page, seed.AIds(12, 7, 3));
        await Assertions.Expect(page.Locator("[class*='skeleton']")).ToHaveCountAsync(0);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J1_Filtreret");

        await ApplyAsync(page, new SearchSpec(Entity: seed.EntityA, Email: seed.EmailX[..^1]));
        await ExpectNoResultsAsync(page);
        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J1_IngenResultater");
    }

    // --- Hjælpere ---

    private async Task<SmokeTestUser> NewAuditorAsync() => await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);

    private async Task<IPage> OpenAsync(int width) =>
        await AuditLogPage.OpenAsync(fixture, await NewAuditorAsync(), width);

    private static DateTimeOffset At(DateOnly day, int dayOffset, int hour, int minute, int second) =>
        new(DateTime.SpecifyKind(day.AddDays(dayOffset).ToDateTime(new TimeOnly(hour, minute, second)), DateTimeKind.Utc));

    /// <summary>Seeder før siden åbnes: dropdownen og første række hentes ved indlæsning.</summary>
    private async Task<Seed> SeedAsync()
    {
        var prefix = Guid.NewGuid().ToString("N")[..8];
        var seed = new Seed(
            prefix,
            AuditLogTestData.NewEntityName("J1a"),
            AuditLogTestData.NewEntityName("J1b"),
            $"j1-{prefix}-x",
            AuditLogTestData.NewUserEmail(),
            $"j1-{prefix}-y",
            AuditLogTestData.NewUserEmail(),
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-30));

        AuditLog Row(string entity, string id, AuditAction action, bool userX, DateTimeOffset ts) =>
            AuditLogTestData.Row(
                entity, id, action, ts,
                userId: userX ? seed.UserIdX : seed.UserIdY,
                userEmail: userX ? seed.EmailX : seed.EmailY,
                ipAddress: "127.0.0.1",
                newValues: "{\"seed\":true}");

        (int N, AuditAction Action, bool X, int Day, int Hour, int Minute, int Second)[] a =
        [
            (1, AuditAction.Create, true, -1, 23, 59, 59),
            (2, AuditAction.Create, true, 0, 0, 0, 0),
            (3, AuditAction.Update, true, 0, 12, 0, 0),
            (4, AuditAction.Update, false, 0, 23, 59, 59),
            (5, AuditAction.Delete, false, 1, 0, 0, 0),
            (6, AuditAction.Create, false, 2, 10, 0, 0),
            (7, AuditAction.Update, true, 3, 10, 0, 0),
            (8, AuditAction.Update, false, 4, 10, 0, 0),
            (9, AuditAction.Delete, true, 5, 10, 0, 0),
            (10, AuditAction.Delete, false, 6, 10, 0, 0),
            (11, AuditAction.Create, true, 7, 10, 0, 0),
            (12, AuditAction.Update, true, 8, 10, 0, 0),
        ];

        (int N, AuditAction Action, bool X, int Day, int Hour, int Minute, int Second)[] b =
        [
            (1, AuditAction.Create, true, 0, 13, 0, 0),
            (2, AuditAction.Update, false, 1, 13, 0, 0),
            (3, AuditAction.Delete, true, 2, 13, 0, 0),
            (4, AuditAction.Update, true, 3, 13, 0, 0),
        ];

        AuditLog[] rows =
        [
            .. a.Select(r => Row(seed.EntityA, $"{prefix}-A{r.N:00}", r.Action, r.X, At(seed.Day, r.Day, r.Hour, r.Minute, r.Second))),
            .. b.Select(r => Row(seed.EntityB, $"{prefix}-B{r.N:00}", r.Action, r.X, At(seed.Day, r.Day, r.Hour, r.Minute, r.Second))),
        ];

        await AuditLogTestData.SeedAsync(fixture.AppConnectionString, rows);

        return seed;
    }

    /// <summary>Sætter ALLE felter (uspecificerede ryddes) og klikker Søg. Sidestørrelsen røres kun, hvis den er angivet.</summary>
    private static async Task ApplyAsync(IPage page, SearchSpec spec)
    {
        await AuditLogPage.SelectEntityAsync(page, spec.Entity ?? string.Empty);
        await AuditLogPage.SetTextAsync(AuditLogPage.EntityIdInput(page), spec.EntityId ?? string.Empty);
        await AuditLogPage.SetTextAsync(AuditLogPage.UserIdInput(page), spec.UserId ?? string.Empty);
        await AuditLogPage.SetTextAsync(AuditLogPage.UserEmailInput(page), spec.Email ?? string.Empty);
        await AuditLogPage.SelectActionAsync(page, spec.Action);
        await AuditLogPage.SetDateAsync(AuditLogPage.FromInput(page), spec.From);
        await AuditLogPage.SetDateAsync(AuditLogPage.ToInput(page), spec.To);

        if (spec.PageSize is { } size)
        {
            await AuditLogPage.SelectPageSizeAsync(page, size);
        }

        await AuditLogPage.SearchAsync(page);
    }

    /// <summary>
    /// Poller, til Entitets-ID-kolonnen præcis matcher listen i visningsrækkefølge. Sammenligning af hele
    /// listen (ikke kun antal) forhindrer, at et gammelt resultat bliver taget for det nye.
    /// </summary>
    private static async Task ExpectRowsAsync(IPage page, params string[] expectedIds)
    {
        IReadOnlyList<string> actual = [];

        var matched = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            var texts = await AuditLogPage.Rows(page).Locator("td:nth-child(5)").AllInnerTextsAsync();
            actual = [.. texts.Select(t => t.Trim())];
            return actual.SequenceEqual(expectedIds);
        }, TimeSpan.FromSeconds(10));

        matched.Should().BeTrue(
            "Entitets-ID'erne skulle være [{0}], men var [{1}]",
            string.Join(", ", expectedIds), string.Join(", ", actual));
    }

    private static async Task ExpectNoResultsAsync(IPage page)
    {
        await ExpectRowsAsync(page);
        await Assertions.Expect(AuditLogPage.EmptyState(page)).ToContainTextAsync("Ingen resultater");
    }

    private static async Task ExpectSavedFieldsAsync(IPage page, Seed seed)
    {
        await Assertions.Expect(AuditLogPage.EntitySelect(page)).ToHaveValueAsync(seed.EntityA);
        await Assertions.Expect(AuditLogPage.FromInput(page)).ToHaveValueAsync(seed.Day.ToString("yyyy-MM-dd"));
        await Assertions.Expect(AuditLogPage.ToInput(page)).ToHaveValueAsync(seed.Day.AddDays(8).ToString("yyyy-MM-dd"));
        await Assertions.Expect(AuditLogPage.PageSizeSelect(page)).ToHaveValueAsync("10");
    }

    /// <summary>Præferencen gemmes efter søgningen uden UI-signal: vent på rækken, før siden genindlæses.</summary>
    private async Task WaitForSavedPreferenceAsync(string userId, int pageSize)
    {
        var saved = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            await using var connection = new SqlConnection(fixture.AppConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(
                "SELECT COUNT(*) FROM [dbo].[AuditLogViewerPreferences] " +
                "WHERE [UserId] = @id AND [PageSize] = @size AND [LastFilterJson] IS NOT NULL", connection);
            command.Parameters.AddWithValue("@id", userId);
            command.Parameters.AddWithValue("@size", pageSize);

            return (int)(await command.ExecuteScalarAsync())! == 1;
        }, TimeSpan.FromSeconds(10));

        saved.Should().BeTrue("præferencen skulle være gemt (sidestørrelse {0}) inden for 10 sekunder", pageSize);
    }
}
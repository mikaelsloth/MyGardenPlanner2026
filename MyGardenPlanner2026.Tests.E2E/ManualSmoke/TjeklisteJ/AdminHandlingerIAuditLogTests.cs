namespace MyGardenPlanner2026.Tests.E2E.ManualSmoke.TjeklisteJ;

using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Tests.E2E.Screenshots;
using System.Text.Json;
using Ids = MyGardenPlanner2026.Tests.E2E.SecurityPolicyPage.FieldIds;

/// <summary>
/// Tjekliste J2: admin CRUD-handlinger (E/F/G) findes som rækker i /admin/audit-log. Hver test udfører selv
/// handlingen med en ny bruger, tæller rækker i admin.AuditLogs før og efter (databasen er delt og kan ikke
/// ryddes) og åbner derefter auditloggen som en frisk AuditViewer, filtrerer på entitet (+ ID, + handling) og
/// læser Old/New JSON via detaljemodalen. Rækkerne findes ved indhold (overgangen Old → New), ikke ved
/// position. Handlende bruger (UserEmail, UserId) og klientens IP-adresse fra circuiten (R1.5) verificeres i
/// Policy_Aendring_RaekkenHarHandlendeBrugerOgIp.
/// Skærmbilleder: Auditlog_J2_Raekker (&lt;bredde&gt;).
/// </summary>
[Collection(PlaywrightAppCollection.Name)]
[Trait(TestCategories.Category, TestCategories.ManualSmoke)]
public sealed class AdminHandlingerIAuditLogTests(PlaywrightAppFixture fixture)
{
    private const string Chapter = "2";
    private const string Checklist = "Tjekliste J";
    private const string TestName = "Auditlog";

    private const string NoData = "(ingen data)";
    private const int Minutes = 30;
    private const int DefaultPageSize = 25;

    private const string PolicyEntity = "AuditLogExportJobPolicySettings";
    private const string PolicyEntityId = "00000000-0000-0000-0000-000000000006";
    private const string TierEntity = "SubscriptionTier";
    private const string AddOnEntity = "SubscriptionAddOn";
    private const string DiscountEntity = "GardenVolumeDiscountTier";
    private const string JitEntity = "RoleElevationRequest";

    private const string LabelCreate = "Oprettet";
    private const string LabelUpdate = "Opdateret";
    private const string LabelDelete = "Slettet";

    private sealed record AuditRow(JsonElement? Old, JsonElement? New);

    // --- a. Policy ---

    [Theory]
    [InlineData(Viewports.Tablet)]
    [InlineData(Viewports.Desktop)]
    public async Task Policy_AendringOgGendannelse_FindesSomUpdateRaekkerMedOldOgNew(int width)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var before = await CountAsync(PolicyEntity, PolicyEntityId, AuditAction.Update);

        var (original, changed) = await ChangeAndRestoreRetentionAsync(user, width);

        var delta = await CountAsync(PolicyEntity, PolicyEntityId, AuditAction.Update) - before;
        delta.Should().Be(2, "ændring og gendannelse giver én Update-række hver");

        var page = await OpenAuditAsync(width, PolicyEntity, PolicyEntityId, AuditAction.Update, LabelUpdate);
        var rows = await ReadRowsAsync(page, delta);

        rows.Should().Contain(
            r => NumberIs(r.Old, "RetentionHours", original) && NumberIs(r.New, "RetentionHours", changed),
            "ændringen skal stå som Old = original, New = ændret");
        rows.Should().Contain(
            r => NumberIs(r.Old, "RetentionHours", changed) && NumberIs(r.New, "RetentionHours", original),
            "gendannelsen skal stå med omvendte værdier");

        await ScreenshotRecorder.CaptureAsync(page, Chapter, Checklist, TestName, "J2_Raekker");
    }

    // --- Handlende bruger ---

    [Fact]
    public async Task Policy_Aendring_RaekkenHarHandlendeBrugerOgIp()
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.PolicyAdmin);
        var userId = await SmokeTestDataSeeder.GetUserIdAsync(fixture.AppConnectionString, user.Email);

        await ChangeAndRestoreRetentionAsync(user, Viewports.Desktop);

        var page = await OpenAuditAsync(Viewports.Desktop, PolicyEntity, PolicyEntityId, AuditAction.Update, LabelUpdate);

        // Nyeste række er gendannelsen, udført af samme bruger.
        var firstRow = AuditLogPage.Rows(page).Nth(0);
        await Assertions.Expect(firstRow.Locator("td:nth-child(2)")).ToHaveTextAsync(user.Email);

        await AuditLogPage.OpenDetailsAsync(firstRow);
        var meta = AuditLogPage.DetailDialog(page).Locator("p.meta");
        await Assertions.Expect(meta).ToContainTextAsync($"({userId})");

        // R1.5: IP-adressen er klientens IP fra circuiten (loopback i E2E), ikke tom.
        await Assertions.Expect(meta).ToContainTextAsync("127.0.0.1");
    }

    // --- b. Volumenrabat: opdatering ---

    [Fact]
    public async Task Volumenrabat_AendringOgGendannelse_FindesSomUpdateRaekker()
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var page = await AdminVolumeDiscountsPage.OpenAsync(fixture, user, Viewports.Desktop);

        const int index = 1;
        var tierId = (await AdminVolumeDiscountsPage.ReadTierIdAsync(page, index)).ToString();
        var minGardens = await AdminVolumeDiscountsPage.ReadMinAsync(page, index);
        var original = await AdminVolumeDiscountsPage.ReadMultiplierAsync(page, index);
        var changed = original == 0.50m ? 0.55m : 0.50m;

        var before = await CountAsync(DiscountEntity, tierId, AuditAction.Update);

        try
        {
            await AdminVolumeDiscountsPage.SetMultiplierAsync(page, index, changed);
            await AdminVolumeDiscountsPage.SaveTierAsync(page, user, index, minGardens);
        }
        finally
        {
            await RestoreMultiplierAsync(user, index, minGardens, original);
        }

        var delta = await CountAsync(DiscountEntity, tierId, AuditAction.Update) - before;
        delta.Should().BeGreaterThanOrEqualTo(2);

        var audit = await OpenAuditAsync(Viewports.Desktop, DiscountEntity, tierId, AuditAction.Update, LabelUpdate);
        var rows = await ReadRowsAsync(audit, delta);

        rows.Should().Contain(
            r => NumberIs(r.Old, "PriceMultiplier", original) && NumberIs(r.New, "PriceMultiplier", changed));
        rows.Should().Contain(
            r => NumberIs(r.Old, "PriceMultiplier", changed) && NumberIs(r.New, "PriceMultiplier", original));
    }

    // --- c. Basispris ---

    [Fact]
    public async Task Basispris_AendringOgGendannelse_FindesSomUpdateRaekker()
    {
        const GardenAccessLevel level = GardenAccessLevel.HaveArkitekt;
        const AccessCategory category = AccessCategory.Editor;

        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var tierId = (await ReadIdAsync(
            "SELECT [Id] FROM [admin].[SubscriptionTiers] WHERE [Level] = @level AND [AccessCategory] = @category AND [IsDeleted] = 0",
            ("@level", (int)level), ("@category", (int)category))).ToString();

        var page = await AdminSubscriptionsPage.OpenAsync(fixture, user, Viewports.Desktop);
        var original = await AdminSubscriptionsPage.ReadPriceAsync(page, BillingCycle.Annual, level, category);
        var changed = original + 1.00m;

        var before = await CountAsync(TierEntity, tierId, AuditAction.Update);

        try
        {
            await AdminSubscriptionsPage.SetPriceAsync(page, BillingCycle.Annual, level, category, changed);
            await AdminSubscriptionsPage.SaveTierAsync(page, user, level, category);
        }
        finally
        {
            await RestorePriceAsync(user, level, category, original);
        }

        var delta = await CountAsync(TierEntity, tierId, AuditAction.Update) - before;
        delta.Should().BeGreaterThanOrEqualTo(2);

        var audit = await OpenAuditAsync(Viewports.Desktop, TierEntity, tierId, AuditAction.Update, LabelUpdate);
        var rows = await ReadRowsAsync(audit, delta);

        rows.Should().Contain(
            r => NumberIs(r.Old, "AnnualPrice", original) && NumberIs(r.New, "AnnualPrice", changed));
        rows.Should().Contain(
            r => NumberIs(r.Old, "AnnualPrice", changed) && NumberIs(r.New, "AnnualPrice", original));
    }

    // --- d. Tilkøb ---

    [Fact]
    public async Task Tilkoeb_AendringOgGendannelse_FindesSomUpdateRaekker()
    {
        const AddOnType type = AddOnType.BedforslagNiveau2;

        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var addOnId = (await ReadIdAsync(
            "SELECT [Id] FROM [admin].[SubscriptionAddOns] WHERE [Type] = @type AND [IsDeleted] = 0",
            ("@type", (int)type))).ToString();

        var page = await AdminAddOnsPage.OpenAsync(fixture, user, Viewports.Desktop);
        var name = await AdminAddOnsPage.ReadNameAsync(page, type);
        var original = await AdminAddOnsPage.ReadPriceAsync(page, type, BillingCycle.Monthly);
        var changed = original + 1.00m;

        var before = await CountAsync(AddOnEntity, addOnId, AuditAction.Update);

        try
        {
            await AdminAddOnsPage.SetPriceAsync(page, type, BillingCycle.Monthly, changed);
            await AdminAddOnsPage.SaveAddOnAsync(page, user, type, name);
        }
        finally
        {
            await RestoreAddOnPriceAsync(user, type, name, original);
        }

        var delta = await CountAsync(AddOnEntity, addOnId, AuditAction.Update) - before;
        delta.Should().BeGreaterThanOrEqualTo(2);

        var audit = await OpenAuditAsync(Viewports.Desktop, AddOnEntity, addOnId, AuditAction.Update, LabelUpdate);
        var rows = await ReadRowsAsync(audit, delta);

        rows.Should().Contain(
            r => NumberIs(r.Old, "MonthlyPrice", original) && NumberIs(r.New, "MonthlyPrice", changed));
        rows.Should().Contain(
            r => NumberIs(r.Old, "MonthlyPrice", changed) && NumberIs(r.New, "MonthlyPrice", original));
    }

    // --- e. Volumenrabat: opret og slet ---

    [Fact]
    public async Task Volumenrabat_OpretOgSlet_FindesSomCreateOgDeleteRaekker()
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.SystemAdmin);
        var page = await AdminVolumeDiscountsPage.OpenAsync(fixture, user, Viewports.Desktop);

        // Højeste MinGardens: trinnet havner som sidste række. Et slettet trin optager stadig sit
        // MinGardens i det unikke indeks, så værdien er tilfældig (se observation i indekset).
        var minGardens = Random.Shared.Next(1000, 9000);
        const decimal multiplier = 0.40m;
        var lastIndex = AdminVolumeDiscountsPage.SeededTierCount;

        string tierId;
        var created = false;
        var deleted = false;

        try
        {
            await AdminVolumeDiscountsPage.AddTierAsync(page, user, minGardens, multiplier);
            created = true;

            await Assertions.Expect(AdminVolumeDiscountsPage.TierRows(page)).ToHaveCountAsync(lastIndex + 1);
            (await AdminVolumeDiscountsPage.ReadMinAsync(page, lastIndex)).Should().Be(minGardens);
            tierId = (await AdminVolumeDiscountsPage.ReadTierIdAsync(page, lastIndex)).ToString();

            await AdminVolumeDiscountsPage.DeleteTierAsync(page, user, lastIndex);
            deleted = true;

            await Assertions.Expect(AdminVolumeDiscountsPage.TierRows(page)).ToHaveCountAsync(lastIndex);
        }
        finally
        {
            if (created && !deleted)
            {
                await AdminVolumeDiscountsPage.DeleteTierAsync(page, user, lastIndex);
            }
        }

        var audit = await OpenAuditAsync(Viewports.Desktop, DiscountEntity, tierId, AuditAction.Create, LabelCreate);
        await Assertions.Expect(AuditLogPage.Rows(audit)).ToHaveCountAsync(1);

        var createRow = (await ReadRowsAsync(audit, 1)).Single();
        createRow.Old.Should().BeNull("en Create-række har ingen tidligere værdier");
        NumberIs(createRow.New, "MinGardens", minGardens).Should().BeTrue();
        NumberIs(createRow.New, "PriceMultiplier", multiplier).Should().BeTrue();
        BoolIs(createRow.New, "IsDeleted", false).Should().BeTrue();

        await FilterAsync(audit, tierId, AuditAction.Delete, LabelDelete, DiscountEntity);
        await Assertions.Expect(AuditLogPage.Rows(audit)).ToHaveCountAsync(1);

        var deleteRow = (await ReadRowsAsync(audit, 1)).Single();
        NumberIs(deleteRow.Old, "MinGardens", minGardens).Should().BeTrue();
        BoolIs(deleteRow.Old, "IsDeleted", false).Should().BeTrue("Old viser tilstanden før sletning");
        deleteRow.New.Should().BeNull("en Delete-række har ingen nye værdier");
    }

    // --- f. JIT ---

    [Fact]
    public async Task Jit_AnmodningGodkendelseOgAfvisning_FindesSomCreateOgUpdateRaekker()
    {
        var requester = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var admin = fixture.SmokeTestUsers[SmokeTestPersonas.Admin];
        var approvedReason = JitRequestsPage.NewReason();
        var rejectedReason = JitRequestsPage.NewReason();

        var before = await CountAsync(JitEntity);

        var requesterPage = await JitRequestsPage.OpenAsync(fixture, requester);
        await JitRequestsPage.RequestAndWaitAsync(requesterPage, RoleNames.SystemAdmin, Minutes, approvedReason);
        await JitRequestsPage.RequestAndWaitAsync(requesterPage, RoleNames.SystemAdmin, Minutes, rejectedReason);

        // To admin-sider: step-up vises kun i en frisk circuit.
        var approvePage = await JitRequestsPage.OpenAsync(fixture, admin);
        await JitRequestsPage.ApproveAsync(approvePage, admin, approvedReason, RoleNames.SystemAdmin);

        var rejectPage = await JitRequestsPage.OpenAsync(fixture, admin);
        await JitRequestsPage.RejectAsync(rejectPage, admin, rejectedReason, RoleNames.SystemAdmin);

        var delta = await CountAsync(JitEntity) - before;
        delta.Should().BeGreaterThanOrEqualTo(4, "2 Create (anmodninger) + 2 Update (godkend, afvis)");

        var audit = await OpenAuditAsync(Viewports.Desktop, JitEntity, entityId: null, action: null, actionLabel: null);
        var rows = await ReadRowsAsync(audit, Math.Min(delta, DefaultPageSize));

        foreach (var reason in new[] { approvedReason, rejectedReason })
        {
            rows.Should().Contain(
                r => r.Old == null && TextIs(r.New, "Reason", reason) && TextIs(r.New, "Status", "Pending"),
                "anmodningen '{0}' skal stå som Create med status Pending", reason);
        }

        rows.Should().Contain(
            r => TextIs(r.Old, "Status", "Pending") && TextIs(r.New, "Status", "Approved")
                && TextIs(r.New, "Reason", approvedReason),
            "godkendelsen skal stå som Update Pending → Approved");
        rows.Should().Contain(
            r => TextIs(r.Old, "Status", "Pending") && TextIs(r.New, "Status", "Rejected")
                && TextIs(r.New, "Reason", rejectedReason),
            "afvisningen skal stå som Update Pending → Rejected");
    }

    // --- Handlinger ---

    private async Task<(int Original, int Changed)> ChangeAndRestoreRetentionAsync(SmokeTestUser user, int width)
    {
        var page = await SecurityPolicyPage.OpenAsync(
            fixture, user, SecurityPolicyPage.ExportJob, Ids.ExportRetention, width);

        var original = await SecurityPolicyPage.ReadFieldAsync(page, Ids.ExportRetention);
        var changed = original > 1 ? original - 1 : original + 1;

        try
        {
            await SecurityPolicyPage.SaveWithStepUpAsync(
                page, user, Ids.ExportRetention, changed, SecurityPolicyPage.ExportJob.SavedText);
        }
        finally
        {
            await SecurityPolicyPage.RestoreFieldAsync(
                fixture, page, SecurityPolicyPage.ExportJob, Ids.ExportRetention, user, original);
        }

        return (original, changed);
    }

    /// <summary>Ny circuit (ny step-up): skriver kun tilbage, hvis værdien afviger fra originalen.</summary>
    private async Task RestoreMultiplierAsync(SmokeTestUser user, int index, int minGardens, decimal original)
    {
        var page = await AdminVolumeDiscountsPage.OpenAsync(fixture, user, Viewports.Desktop);

        if (await AdminVolumeDiscountsPage.ReadMultiplierAsync(page, index) != original)
        {
            await AdminVolumeDiscountsPage.SetMultiplierAsync(page, index, original);
            await AdminVolumeDiscountsPage.SaveTierAsync(page, user, index, minGardens);
        }

        (await AdminVolumeDiscountsPage.ReadMultiplierAsync(page, index)).Should().Be(original);
    }

    private async Task RestorePriceAsync(
        SmokeTestUser user, GardenAccessLevel level, AccessCategory category, decimal original)
    {
        var page = await AdminSubscriptionsPage.OpenAsync(fixture, user, Viewports.Desktop);

        if (await AdminSubscriptionsPage.ReadPriceAsync(page, BillingCycle.Annual, level, category) != original)
        {
            await AdminSubscriptionsPage.SetPriceAsync(page, BillingCycle.Annual, level, category, original);
            await AdminSubscriptionsPage.SaveTierAsync(page, user, level, category);
        }

        (await AdminSubscriptionsPage.ReadPriceAsync(page, BillingCycle.Annual, level, category))
            .Should().Be(original);
    }

    private async Task RestoreAddOnPriceAsync(SmokeTestUser user, AddOnType type, string name, decimal original)
    {
        var page = await AdminAddOnsPage.OpenAsync(fixture, user, Viewports.Desktop);

        if (await AdminAddOnsPage.ReadPriceAsync(page, type, BillingCycle.Monthly) != original)
        {
            await AdminAddOnsPage.SetPriceAsync(page, type, BillingCycle.Monthly, original);
            await AdminAddOnsPage.SaveAddOnAsync(page, user, type, name);
        }

        (await AdminAddOnsPage.ReadPriceAsync(page, type, BillingCycle.Monthly)).Should().Be(original);
    }

    // --- AuditLog-siden ---

    /// <summary>
    /// Åbner siden med en frisk AuditViewer EFTER handlingerne (dropdownen populeres ved indlæsning) og
    /// filtrerer. actionLabel = null betyder, at handlingen ikke afgrænses.
    /// </summary>
    private async Task<IPage> OpenAuditAsync(
        int width, string entity, string? entityId, AuditAction? action, string? actionLabel)
    {
        var user = await fixture.SeedExtraUserAsync(RoleNames.AuditViewer);
        var page = await AuditLogPage.OpenAsync(fixture, user, width);

        await FilterAsync(page, entityId, action, actionLabel, entity);

        return page;
    }

    private static async Task FilterAsync(
        IPage page, string? entityId, AuditAction? action, string? actionLabel, string entity)
    {
        await AuditLogPage.SelectEntityAsync(page, entity);
        await AuditLogPage.SetTextAsync(AuditLogPage.EntityIdInput(page), entityId ?? string.Empty);
        await AuditLogPage.SelectActionAsync(page, action);
        await AuditLogPage.SearchAsync(page);

        await ExpectFilteredAsync(page, entity, entityId, actionLabel);
    }

    /// <summary>
    /// Poller, til ALLE viste rækker matcher filteret (og der er mindst én). Forhindrer, at det
    /// ufiltrerede førsteresultat læses, før søgningen er slået igennem.
    /// </summary>
    private static async Task ExpectFilteredAsync(IPage page, string entity, string? entityId, string? actionLabel)
    {
        var matched = await BlazorInteraction.WaitUntilAsync(async () =>
        {
            var rows = AuditLogPage.Rows(page);
            var entities = await TextsAsync(rows.Locator("td:nth-child(4)"));

            return entities.Count != 0 && !entities.Any(e => e != entity) && (entityId is null
                || !(await TextsAsync(rows.Locator("td:nth-child(5)"))).Any(i => i != entityId)) && (actionLabel is null
                || (await TextsAsync(rows.Locator("td:nth-child(3)"))).All(l => l == actionLabel));
        }, TimeSpan.FromSeconds(10));

        matched.Should().BeTrue(
            "tabellen skulle kun vise rækker for entitet '{0}', ID '{1}', handling '{2}'",
            entity, entityId ?? "(alle)", actionLabel ?? "(alle)");
    }

    private static async Task<List<string>> TextsAsync(ILocator cells) =>
        [.. (await cells.AllInnerTextsAsync()).Select(t => t.Trim())];

    /// <summary>Læser Old/New for de øverste rækker via detaljemodalen (nyeste først).</summary>
    private static async Task<IReadOnlyList<AuditRow>> ReadRowsAsync(IPage page, int count)
    {
        var rows = new List<AuditRow>();
        var available = await AuditLogPage.Rows(page).CountAsync();

        for (var i = 0; i < Math.Min(count, available); i++)
        {
            await AuditLogPage.OpenDetailsAsync(AuditLogPage.Rows(page).Nth(i));
            await Assertions.Expect(AuditLogPage.DetailDialog(page)).ToBeVisibleAsync();

            var oldText = await ReadBlockAsync(page, 0);
            var newText = await ReadBlockAsync(page, 1);
            rows.Add(new AuditRow(ParseOrNull(oldText), ParseOrNull(newText)));

            await AuditLogPage.CloseButton(page).ClickAsync();
            await Assertions.Expect(AuditLogPage.DetailDialog(page)).ToBeHiddenAsync();
        }

        return rows;
    }

    private static async Task<string> ReadBlockAsync(IPage page, int index) =>
        ((await AuditLogPage.JsonBlock(page, index).TextContentAsync()) ?? string.Empty).Replace("\r\n", "\n");

    private static JsonElement? ParseOrNull(string text)
    {
        if (text == NoData)
        {
            return null;
        }

        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    // --- JSON-prædikater ---

    private static bool NumberIs(JsonElement? json, string property, decimal expected) =>
        Property(json, property) is { ValueKind: JsonValueKind.Number } value && value.GetDecimal() == expected;

    private static bool TextIs(JsonElement? json, string property, string expected) =>
        Property(json, property) is { ValueKind: JsonValueKind.String } value && value.GetString() == expected;

    private static bool BoolIs(JsonElement? json, string property, bool expected) =>
        Property(json, property) is { } value
        && value.ValueKind == (expected ? JsonValueKind.True : JsonValueKind.False);

    private static JsonElement? Property(JsonElement? json, string property) =>
        json is { ValueKind: JsonValueKind.Object } element && element.TryGetProperty(property, out var value)
            ? value
            : null;

    // --- Database ---

    private Task<int> CountAsync(string entity, string? entityId = null, AuditAction? action = null) =>
        AuditLogTestData.CountForEntityAsync(fixture.AppConnectionString, entity, entityId, action);

    private async Task<Guid> ReadIdAsync(string sql, params (string Name, int Value)[] parameters)
    {
        await using var connection = new SqlConnection(fixture.AppConnectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return (Guid)(await command.ExecuteScalarAsync()
            ?? throw new InvalidOperationException($"Ingen række fundet for: {sql}"));
    }
}
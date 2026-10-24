namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

/// <summary>
/// Page-object for /admin/audit-log. Siden er InteractiveServer: input ignoreres, til kredsløbet er
/// forbundet, og prerendering kan nulstille indtastninger. OpenAsync venter derfor på interaktivitet,
/// FØR testen indtaster noget. Udfyldning af @bind-felter skal efterfølges af blur.
/// </summary>
public static class AuditLogPage
{
    public const string Title = "AuditLog – MyGardenPlanner";
    public const string Heading = "AuditLog";

    // --- Åbning ---

    /// <summary>
    /// Logger ind, åbner siden og venter, til den er interaktiv. Installerer opsnapning af window.open
    /// (se <see cref="AuditLogEport"/>) FØR første navigation.
    /// </summary>
    public static async Task<IPage> OpenAsync(PlaywrightAppFixture fixture, SmokeTestUser user, int? width = null)
    {
        var page = width is null ? await fixture.NewPageAsync() : await fixture.NewPageAsync((int)width);
        await AuditLogExport.InstallCaptureAsync(page);

        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.AdminAuditLog}");
        await LoginFlow.LoginAsync(page, user);

        await SmokePageAssertions.ExpectPageAsync(page, Title, Heading);
        await WaitUntilInteractiveAsync(page);

        return page;
    }

    /// <summary>Genindlæser siden (ny circuit) og venter, til den er interaktiv.</summary>
    public static async Task ReloadAsync(IPage page)
    {
        await page.ReloadAsync();
        await SmokePageAssertions.ExpectPageAsync(page, Title, Heading);
        await WaitUntilInteractiveAsync(page);
    }

    /// <summary>
    /// Åbner og lukker detaljemodalen på første række: den er idempotent og viser tydeligt, når kredsløbet
    /// er forbundet. Kræver mindst én række i tabellen (seed før siden åbnes).
    /// </summary>
    public static async Task WaitUntilInteractiveAsync(IPage page)
    {
        var firstDetails = ResultsTable(page).Locator("tbody tr").First
            .GetByRole(AriaRole.Button, new() { Name = "Detaljer", Exact = true });

        await Assertions.Expect(firstDetails).ToBeVisibleAsync();

        await BlazorInteraction.ActUntilAsync(
            () => firstDetails.ClickAsync(),
            async () => await DetailDialog(page).IsVisibleAsync(),
            maxAttempts: 12,
            description: "AuditLog-siden blev ikke interaktiv");

        await CloseButton(page).ClickAsync();
        await Assertions.Expect(DetailDialog(page)).ToBeHiddenAsync();
    }

    // --- Filter ---

    public static ILocator SearchButton(IPage page) => page.Locator(".filter-bar-actions button");

    public static ILocator EntitySelect(IPage page) => page.Locator("#audit-filter-entity");

    public static ILocator ActionSelect(IPage page) => page.Locator("#audit-filter-action");

    public static ILocator PageSizeSelect(IPage page) => page.Locator("#audit-filter-pagesize");

    public static ILocator EntityIdInput(IPage page) => page.Locator("#audit-filter-entity-id");

    public static ILocator UserIdInput(IPage page) => page.Locator("#audit-filter-user-id");

    public static ILocator UserEmailInput(IPage page) => page.Locator("#audit-filter-user-email");

    public static ILocator FromInput(IPage page) => page.Locator("#audit-filter-from");

    public static ILocator ToInput(IPage page) => page.Locator("#audit-filter-to");

    /// <summary>Tom streng = "Alle".</summary>
    public static Task SelectEntityAsync(IPage page, string entityName) =>
        EntitySelect(page).SelectOptionAsync(entityName);

    public static Task SelectActionAsync(IPage page, AuditAction? action) =>
        ActionSelect(page).SelectOptionAsync(action?.ToString() ?? string.Empty);

    public static Task SelectPageSizeAsync(IPage page, int size) =>
        PageSizeSelect(page).SelectOptionAsync(size.ToString(CultureInfo.InvariantCulture));

    /// <summary>@bind udløses ved blur.</summary>
    public static async Task SetTextAsync(ILocator input, string value)
    {
        await input.FillAsync(value);
        await input.BlurAsync();
    }

    /// <summary>Dato som yyyy-MM-dd (type=date). Tom streng rydder feltet.</summary>
    public static Task SetDateAsync(ILocator input, DateOnly? date) =>
        SetTextAsync(input, date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty);

    public static Task SearchAsync(IPage page) => SearchButton(page).ClickAsync();

    // --- Resultater ---

    /// <summary>Resultattabellen (kolonnen "Entitets-ID" adskiller den fra tabellen over baggrundseksporter).</summary>
    public static ILocator ResultsTable(IPage page) =>
        page.Locator("table.data-table")
            .Filter(new() { Has = page.GetByRole(AriaRole.Columnheader, new() { Name = "Entitets-ID", Exact = true }) });

    public static ILocator Rows(IPage page) => ResultsTable(page).Locator("tbody tr");

    public static ILocator RowsFor(IPage page, string entityName) =>
        Rows(page).Filter(new() { HasText = entityName });

    /// <summary>Kolonner: 0 Tidspunkt, 1 Bruger, 2 Handling, 3 Entitet, 4 Entitets-ID, 5 IP-adresse, 6 Detaljer.</summary>
    public static ILocator Cell(ILocator row, int column) => row.Locator("td").Nth(column);

    public static ILocator EmptyState(IPage page) => page.Locator(".empty-state.empty-search");

    public static ILocator PaginationInfo(IPage page) => page.Locator(".audit-log-pagination .meta");

    public static ILocator PreviousButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Forrige", Exact = true });

    public static ILocator NextButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Næste", Exact = true });

    // --- Detaljemodal ---

    public static ILocator DetailDialog(IPage page) => page.Locator(".audit-log-detail-dialog");

    public static ILocator DetailBackdrop(IPage page) => page.Locator(".confirm-dialog-backdrop");

    public static ILocator CloseButton(IPage page) =>
        DetailDialog(page).GetByRole(AriaRole.Button, new() { Name = "Luk", Exact = true });

    /// <summary>0 = Tidligere værdier, 1 = Nye værdier.</summary>
    public static ILocator JsonBlock(IPage page, int index) => DetailDialog(page).Locator(".audit-log-detail-json").Nth(index);

    public static Task OpenDetailsAsync(ILocator row) =>
        row.GetByRole(AriaRole.Button, new() { Name = "Detaljer", Exact = true }).ClickAsync();

    // --- Eksportpanel ---

    public static ILocator ExportPanel(IPage page) => page.Locator(".audit-log-export-panel");

    public static ILocator ExportFormatSelect(IPage page) => page.Locator("#export-format");

    public static ILocator ExportButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Eksportér", Exact = true });

    public static ILocator BackgroundExportButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Send til baggrundseksport", Exact = true });

    public static ILocator ExportError(IPage page) => ExportPanel(page).Locator(".status-danger");

    public static ILocator ExportSuccess(IPage page) => ExportPanel(page).Locator(".status-success");

    public static ILocator ThresholdConfirm(IPage page) => ExportPanel(page).Locator(".inline-confirm");

    public static ILocator ThresholdConfirmButton(IPage page) =>
        ThresholdConfirm(page).GetByRole(AriaRole.Button, new() { Name = "Ja, fortsæt", Exact = true });

    public static ILocator ThresholdCancelButton(IPage page) =>
        ThresholdConfirm(page).GetByRole(AriaRole.Button, new() { Name = "Annullér", Exact = true });

    public static Task SelectExportFormatAsync(IPage page, AuditLogExportFormat format) =>
        ExportFormatSelect(page).SelectOptionAsync(format.ToString());

    // --- Baggrundseksporter ---

    public static ILocator JobRows(IPage page) => page.Locator(".audit-log-export-jobs-panel tbody tr");
}
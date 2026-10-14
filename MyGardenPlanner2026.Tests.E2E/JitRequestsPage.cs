namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;
using System.Globalization;

/// <summary>
/// Page-object for /admin/jit-requests (fanerne "Anmod om adgang" og "Godkend anmodninger").
/// Siden er InteractiveServer: klik og input ignoreres, til kredsløbet er forbundet.
/// OpenAsync venter derfor på, at fanehop virker, og afslutter på fanen "Anmod om adgang".
/// "Send anmodning" er ikke idempotent og må ikke gentages — klikkes derfor kun én gang,
/// efter at siden har bevist, at den er interaktiv.
/// </summary>
public static class JitRequestsPage
{
    public const string Title = "JIT-adgang – MyGardenPlanner";
    public const string Heading = "JIT-adgang";

    public const string RequestTabName = "Anmod om adgang";
    public const string ApproveTabName = "Godkend anmodninger";

    public const string PendingLabel = "Afventer";
    public const string ApprovedLabel = "Godkendt";
    public const string RejectedLabel = "Afvist";

    private const string RequestHeadingText = "Anmod om midlertidig adgang";
    private const string QueueHeadingText = "Anmodninger til godkendelse";

    /// <summary>Unik årsag, så en række kan genkendes på tværs af delte personas og tests.</summary>
    public static string NewReason() => $"e2e-{Guid.NewGuid():N}";

    /// <summary>Logger ind, åbner siden og venter, til den er interaktiv. Slutter på fanen "Anmod om adgang".</summary>
    public static async Task<IPage> OpenAsync(PlaywrightAppFixture fixture, SmokeTestUser user, int? width = null)
    {
        var page = await fixture.LoginAndGotoAsync(user, E2ERoutes.AdminJitRequests, width);

        await SmokePageAssertions.ExpectPageAsync(page, Title, Heading);
        await WaitUntilInteractiveAsync(page);

        return page;
    }

    public static Task<IPage> OpenAsync(PlaywrightAppFixture fixture, string persona, int? width = null) =>
        OpenAsync(fixture, fixture.SmokeTestUsers[persona], width);

    /// <summary>Åbner siden i en allerede logget ind side og venter, til den er interaktiv. Slutter på "Anmod om adgang".</summary>
    public static async Task GotoAsync(PlaywrightAppFixture fixture, IPage page)
    {
        await page.GotoAsync($"{fixture.RootUri}{E2ERoutes.AdminJitRequests}");

        await SmokePageAssertions.ExpectPageAsync(page, Title, Heading);
        await WaitUntilInteractiveAsync(page);
    }

    /// <summary>
    /// Kun en interaktiv side kan skifte fane. Godkend-fanens overskrift findes ikke i prerenderet
    /// HTML, så den beviser, at kredsløbet er forbundet. Derefter tilbage til anmod-fanen.
    /// </summary>
    public static async Task WaitUntilInteractiveAsync(IPage page)
    {
        await SelectTabAsync(page, ApproveTabName);
        await Assertions.Expect(QueueHeading(page)).ToBeVisibleAsync();

        await SelectTabAsync(page, RequestTabName);
        await Assertions.Expect(RequestHeading(page)).ToBeVisibleAsync();
    }

    public static ILocator Tab(IPage page, string name) =>
        page.GetByRole(AriaRole.Tab, new() { Name = name, Exact = true });

    /// <summary>Vælger en fane og gentager klikket, til fanen er aktiv (klassen "active").</summary>
    public static Task SelectTabAsync(IPage page, string name)
    {
        var tab = Tab(page, name);

        return BlazorInteraction.ActUntilAsync(
            () => tab.ClickAsync(),
            async () =>
            {
                var classes = await tab.GetAttributeAsync("class") ?? string.Empty;
                return classes.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("active");
            },
            maxAttempts: 12,
            description: $"Fanen '{name}' blev ikke valgt");
    }

    public static ILocator RequestHeading(IPage page) =>
        page.GetByRole(AriaRole.Heading, new() { Name = RequestHeadingText });

    public static ILocator QueueHeading(IPage page) =>
        page.GetByRole(AriaRole.Heading, new() { Name = QueueHeadingText });

    // --- Anmod-fanen ---

    public static ILocator RoleSelect(IPage page) => page.Locator("#jit-request-role");

    public static ILocator MinutesInput(IPage page) => page.Locator("#jit-request-minutes");

    public static ILocator ReasonInput(IPage page) => page.Locator("#jit-request-reason");

    public static ILocator SubmitButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Send anmodning", Exact = true });

    /// <summary>Fejlbesked fra formularen (role=alert), fx policy-grænser.</summary>
    public static ILocator Error(IPage page) =>
        page.Locator(".jit-elevation-request-form .status-danger");

    /// <summary>Række i "Mine anmodninger" med den angivne årsag.</summary>
    public static ILocator RequestRow(IPage page, string reason) =>
        page.Locator(".jit-elevation-request-form tbody tr").Filter(new() { HasText = reason });

    public static ILocator StatusBadge(ILocator row) => row.Locator(".badge-role");

    /// <summary>Udfylder formularen og klikker "Send anmodning" én gang. Venter ikke på resultatet.</summary>
    public static async Task SubmitRequestAsync(IPage page, string role, int minutes, string reason)
    {
        await RoleSelect(page).SelectOptionAsync(role);
        await SetValueAsync(MinutesInput(page), minutes.ToString(CultureInfo.InvariantCulture));
        await SetValueAsync(ReasonInput(page), reason);

        await SubmitButton(page).ClickAsync();
    }

    /// <summary>Som <see cref="SubmitRequestAsync"/>, men venter, til rækken vises i "Mine anmodninger".</summary>
    public static async Task RequestAndWaitAsync(IPage page, string role, int minutes, string reason)
    {
        await SubmitRequestAsync(page, role, minutes, reason);
        await Assertions.Expect(RequestRow(page, reason)).ToBeVisibleAsync();
    }

    /// <summary>Krydser "Vis afviste/udløbne" af (viser også Afvist og Udløbet).</summary>
    public static Task ShowHistoryAsync(IPage page) =>
        page.Locator("#jit-request-show-history").CheckAsync();

    /// <summary>
    /// Genindlæser "Mine anmodninger" uden sideindlæsning: fanehop til Godkend og tilbage
    /// opretter formularen på ny (listen hentes igen). "Vis afviste/udløbne" nulstilles ved hop,
    /// derfor kan den krydses af igen.
    /// </summary>
    public static async Task RefreshRequestsAsync(IPage page, bool showHistory = false)
    {
        await SelectTabAsync(page, ApproveTabName);
        await SelectTabAsync(page, RequestTabName);
        await Assertions.Expect(RequestHeading(page)).ToBeVisibleAsync();

        if (showHistory)
        {
            await ShowHistoryAsync(page);
        }
    }

    // --- Godkend-fanen ---

    /// <summary>Række i godkendelseskøen med den angivne årsag.</summary>
    public static ILocator QueueRow(IPage page, string reason) =>
        page.Locator(".jit-elevation-approval-queue tbody tr").Filter(new() { HasText = reason });

    public static ILocator ApproveButton(IPage page, string reason) =>
    QueueRow(page, reason).GetByRole(AriaRole.Button, new() { Name = "Godkend", Exact = true });

    public static ILocator RejectButton(IPage page, string reason) =>
        QueueRow(page, reason).GetByRole(AriaRole.Button, new() { Name = "Afvis", Exact = true });

    /// <summary>Statusbeskeden på siden efter godkend/afvis (ryddes ved fanehop).</summary>
    public static ILocator StatusMessage(IPage page) => page.Locator(".status-success");

    /// <summary>Godkender rækken med step-up. Siden skal være åbnet via <see cref="OpenAsync(PlaywrightAppFixture, SmokeTestUser, int?)"/>.</summary>
    public static Task ApproveAsync(IPage page, SmokeTestUser approver, string reason, string role) =>
        DecideAsync(page, approver, reason, ApproveButton(page, reason), $"Anmodning om '{role}' er godkendt.");

    /// <summary>Afviser rækken med step-up.</summary>
    public static Task RejectAsync(IPage page, SmokeTestUser approver, string reason, string role) =>
        DecideAsync(page, approver, reason, RejectButton(page, reason), $"Anmodning om '{role}' er afvist.");

    /// <summary>
    /// Åbner køen, venter på rækken, klikker (gentages, til step-up modalen vises — en frisk circuit
    /// har intet re-auth-tidsstempel), gennemfører step-up og afventer beskeden og at rækken er væk.
    /// </summary>
    private static async Task DecideAsync(
        IPage page, SmokeTestUser approver, string reason, ILocator button, string expectedMessage)
    {
        await SelectTabAsync(page, ApproveTabName);
        await Assertions.Expect(QueueRow(page, reason)).ToBeVisibleAsync();

        await StepUpModal.OpenAsync(page, () => button.ClickAsync());
        await StepUpModal.CompleteAsync(page, approver);

        await Assertions.Expect(StatusMessage(page)).ToContainTextAsync(expectedMessage);
        await Assertions.Expect(QueueRow(page, reason)).ToHaveCountAsync(0);
    }

    // @bind/@onchange udløses først ved blur.
    private static async Task SetValueAsync(ILocator input, string value)
    {
        await input.FillAsync(value);
        await input.BlurAsync();
    }
}
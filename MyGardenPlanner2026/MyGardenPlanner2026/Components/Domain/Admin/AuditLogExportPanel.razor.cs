namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Eksporterer det aktuelle søgeresultat (samme filter som visningen, uden paginering)
/// til CSV/JSON/Excel. Beskyttet af StepUpGuard (følsomme data: IP-adresser, e-mails,
/// fulde Old/New-værdier) og AdminActionGuard (rate limiting). Download sker som en
/// almindelig HTTP GET mod /admin/audit-log/export, autoriseret ved et korttidsholdbart,
/// signeret token udstedt EFTER StepUpGuard er bestået (se IAuditLogExportTokenService
/// — nødvendigt fordi IReAuthenticationService er circuit-scoped og ikke tilgængeligt
/// fra endpointets eget HTTP-scope).
///
/// Ved > 50.000 matchende rækker vises en eksplicit bekræftelsesdialog. Ved
/// > IAuditLogExportService.HardRowLimit (250.000) afvises eksporten. Bekræftelses-
/// klikket gentager BEVIDST ikke StepUpGuard/AdminActionGuard — det er en fortsættelse
/// af samme brugerhandling, ikke en ny admin-handling.
/// </summary>
public partial class AuditLogExportPanel
{
    private const int SoftRowLimitThreshold = 50_000;

    [Inject]
    private IAuditLogQueryService QueryService { get; set; } = default!;

    [Inject]
    private IAuditLogExportTokenService TokenService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private ILogger<AuditLogExportPanel> Logger { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter, EditorRequired]
    public AuditLogFilterDto CurrentFilter { get; set; } = default!;

    private AuditLogExportFormat selectedFormat = AuditLogExportFormat.Csv;
    private string? errorMessage;
    private bool showThresholdConfirm;
    private int pendingRowCount;
    private StepUpGuard stepUpGuard = default!;
    private AdminActionGuard adminActionGuard = default!;

    [LoggerMessage(EventId = 1042, Level = LogLevel.Information, Message = "Bruger '{UserId}' anmodede om AuditLog-eksport i format '{Format}'.")]
    static partial void ExportRequested(ILogger logger, string UserId, AuditLogExportFormat Format);

    [LoggerMessage(EventId = 1043, Level = LogLevel.Warning, Message = "AuditLog-eksport afvist for bruger '{UserId}': {Count} rækker overstiger den tilladte grænse på {Limit}.")]
    static partial void ExportHardLimitExceeded(ILogger logger, string UserId, int Count, int Limit);

    [LoggerMessage(EventId = 1044, Level = LogLevel.Information, Message = "AuditLog-eksport for bruger '{UserId}' kræver bekræftelse: {Count} rækker overstiger det bløde tærskel.")]
    static partial void ExportSoftLimitThresholdReached(ILogger logger, string UserId, int Count);

    [LoggerMessage(EventId = 1045, Level = LogLevel.Information, Message = "Bruger '{UserId}' udløste download af AuditLog-eksport i format '{Format}'.")]
    static partial void ExportDownloadTriggered(ILogger logger, string UserId, AuditLogExportFormat Format);

    [LoggerMessage(EventId = 1046, Level = LogLevel.Warning, Message = "Kunne ikke bestemme den aktuelle bruger ved AuditLog-eksport.")]
    static partial void ExportCurrentUserCouldNotBeResolved(ILogger logger);

    protected override void OnInitialized()
    {
        stepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        adminActionGuard = new AdminActionGuard(RateLimiter);
    }

    private async Task ExportButtonClickedAsync()
    {
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, CheckAndExportCoreAsync));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task CheckAndExportCoreAsync()
    {
        errorMessage = null;
        showThresholdConfirm = false;

        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        if (userId is not null)
        {
            ExportRequested(Logger, userId, selectedFormat);
        }

        var count = await QueryService.CountAsync(CurrentFilter);

        if (count > IAuditLogExportService.HardRowLimit)
        {
            errorMessage = $"Error: Eksporten omfatter {count} rækker, hvilket overstiger grænsen på " +
                $"{IAuditLogExportService.HardRowLimit}. Indsnævr filteret og prøv igen.";
            ExportHardLimitExceeded(Logger, userId ?? "ukendt", count, IAuditLogExportService.HardRowLimit);
            return;
        }

        if (count > SoftRowLimitThreshold)
        {
            pendingRowCount = count;
            showThresholdConfirm = true;
            ExportSoftLimitThresholdReached(Logger, userId ?? "ukendt", count);
            return;
        }

        await TriggerDownloadAsync();
    }

    private async Task ConfirmExportAsync()
    {
        showThresholdConfirm = false;
        await TriggerDownloadAsync();
    }

    private void CancelExportConfirm()
    {
        showThresholdConfirm = false;
        pendingRowCount = 0;
    }

    private async Task TriggerDownloadAsync()
    {
        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        if (userId is null)
        {
            errorMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";
            ExportCurrentUserCouldNotBeResolved(Logger);
            return;
        }

        var token = TokenService.IssueToken(userId);
        var url = BuildExportUrl(token);

        ExportDownloadTriggered(Logger, userId, selectedFormat);

        await JS.InvokeVoidAsync("open", url, "_blank");
    }

    private string BuildExportUrl(string token)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["format"] = selectedFormat.ToString(),
            ["token"] = token,
            ["entityName"] = CurrentFilter.EntityName,
            ["entityId"] = CurrentFilter.EntityId,
            ["userId"] = CurrentFilter.UserId,
            ["userEmail"] = CurrentFilter.UserEmail,
            ["action"] = CurrentFilter.Action?.ToString(),
            ["fromUtc"] = CurrentFilter.FromUtc?.ToString("O"),
            ["toUtc"] = CurrentFilter.ToUtc?.ToString("O")
        };

        var query = string.Join('&', queryParams
            .Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));

        return NavigationManager.ToAbsoluteUri($"/admin/audit-log/export?{query}").ToString();
    }
}
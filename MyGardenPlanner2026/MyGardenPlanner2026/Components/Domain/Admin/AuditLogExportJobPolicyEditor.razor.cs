namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;

/// <summary>
/// Ændring af retention-/maks.-aktive-policyen for AuditLog-baggrundseksporter er en
/// følsom "core policy"-handling (§3.2) og kræver derfor step-up re-autentificering —
/// håndhævet via StepUpGuard, samme mønster som de øvrige sikkerhedspolicy-editors.
/// </summary>
public partial class AuditLogExportJobPolicyEditor
{
    [Inject]
    private IAuditLogExportJobPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [Inject]
    private ILogger<AuditLogExportJobPolicyEditor> Logger { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private AuditLogExportJobPolicyDto? settings;
    private int retentionHoursEdit;
    private int maxActiveEdit;
    private string? errorMessage;
    private StepUpGuard stepUpGuard = default!;
    private AdminActionGuard adminActionGuard = default!;

    [LoggerMessage(EventId = 1130, Level = LogLevel.Information, Message = "Opdatering af AuditLog-eksportjob-policy afvist: {Reason}")]
    static partial void ExportJobPolicySaveFailed(ILogger logger, string Reason);

    protected override async Task OnInitializedAsync()
    {
        stepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        adminActionGuard = new AdminActionGuard(RateLimiter);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        settings = await AdminService.GetAsync();
        retentionHoursEdit = settings.RetentionHours;
        maxActiveEdit = settings.MaxActiveJobsPerUser;
    }

    private async Task SaveAsync()
    {
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, SaveCoreAsync));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task SaveCoreAsync()
    {
        errorMessage = null;
        try
        {
            var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            if (userId is null)
            {
                errorMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";
                return;
            }

            await AdminService.UpdateAsync(new AuditLogExportJobPolicyDto(retentionHoursEdit, maxActiveEdit), userId);

            await LoadAsync();
            await OnStatusMessage.InvokeAsync("AuditLog-eksportjob-policyen er opdateret og trådt i kraft.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            errorMessage = $"Error: {ex.Message}";
            ExportJobPolicySaveFailed(Logger, ex.Message);
        }
    }

    private static int ParseInt(object? value) =>
        int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var result) ? result : 0;
}
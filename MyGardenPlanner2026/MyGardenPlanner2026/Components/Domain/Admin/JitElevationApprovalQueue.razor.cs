namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;

/// <summary>
/// Viser ventende JIT-anmodninger som den aktuelle bruger kan godkende (peer med
/// matchende rolle, se IJitElevationService.GetPendingRequestsForApprovalAsync).
/// Godkend/Afvis giver reel adgang og er derfor en følsom "core policy"-handling
/// (§3.2) — håndhævet via StepUpGuard, samme mønster som de øvrige admin-editors.
/// </summary>
public partial class JitElevationApprovalQueue
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    [Inject]
    private IJitElevationService JitElevationService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<RoleElevationRequestDto> pendingRequests = [];
    private string? errorMessage;
    private StepUpGuard stepUpGuard = default!;
    private AdminActionGuard adminActionGuard = default!;

    protected override async Task OnInitializedAsync()
    {
        stepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        adminActionGuard = new AdminActionGuard(RateLimiter);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var approverId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        pendingRequests = approverId is null
            ? []
            : await JitElevationService.GetPendingRequestsForApprovalAsync(approverId);
    }

    private async Task ApproveAsync(Guid requestId)
    {
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, () => ApproveCoreAsync(requestId)));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task ApproveCoreAsync(Guid requestId)
    {
        errorMessage = null;
        try
        {
            var approverId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            if (approverId is null)
            {
                errorMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";
                return;
            }

            var approved = await JitElevationService.ApproveElevationAsync(approverId, requestId);

            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Anmodning om '{approved.RoleName}' er godkendt.");
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }

    private async Task RejectAsync(Guid requestId)
    {
        await adminActionGuard.RunAsync(AuthenticationStateTask, () =>
            stepUpGuard.RunAsync(AuthenticationStateTask, () => RejectCoreAsync(requestId)));

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task RejectCoreAsync(Guid requestId)
    {
        errorMessage = null;
        try
        {
            var approverId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            if (approverId is null)
            {
                errorMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";
                return;
            }

            var rejected = await JitElevationService.RejectElevationAsync(approverId, requestId);

            await LoadAsync();
            await OnStatusMessage.InvokeAsync($"Anmodning om '{rejected.RoleName}' er afvist.");
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }
}
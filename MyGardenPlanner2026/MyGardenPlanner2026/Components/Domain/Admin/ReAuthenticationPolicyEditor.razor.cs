namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;

/// <summary>
/// Ændring af step-up re-autentificeringspolicyen er en følsom "core policy"-handling
/// (§3.2) og kræver derfor step-up re-autentificering — håndhævet via StepUpGuard.
/// </summary>
public partial class ReAuthenticationPolicyEditor
{
    [Inject]
    private IReAuthenticationPolicyAdminService AdminService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private ReAuthenticationPolicyDto? settings;
    private int maxAgeEdit;
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
        settings = await AdminService.GetAsync();
        maxAgeEdit = settings.MaxAgeMinutes;
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

            await AdminService.UpdateAsync(new ReAuthenticationPolicyDto(maxAgeEdit), userId);

            await LoadAsync();
            await OnStatusMessage.InvokeAsync("Step-up re-auth-policyen er opdateret og trådt i kraft.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }

    private static int ParseInt(object? value) =>
        int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var result) ? result : 0;
}
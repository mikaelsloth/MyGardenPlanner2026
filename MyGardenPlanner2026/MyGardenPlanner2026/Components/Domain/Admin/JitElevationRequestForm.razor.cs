namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

/// <summary>
/// Lader en autentificeret bruger anmode om midlertidig (JIT) forhøjelse til en af de
/// fire admin-roller, og viser brugerens egne anmodninger. Selve anmodningen kræver IKKE
/// step-up re-authentication (den giver ingen adgang i sig selv — det gør først en peer-
/// godkendelse, se JitElevationApprovalQueue), men er rate-limited via AdminActionGuard
/// for at forhindre spam.
/// </summary>
public partial class JitElevationRequestForm
{
    private static readonly CultureInfo DanishCulture = new("da-DK");

    private static readonly string[] AdminRoles =
    [RoleNames.SystemAdmin, RoleNames.DataAdmin, RoleNames.PolicyAdmin, RoleNames.AuditViewer];

    [Inject]
    private IJitElevationService JitElevationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private string selectedRole = RoleNames.SystemAdmin;
    private int minutes = 30;
    private string reason = string.Empty;
    private string? errorMessage;
    private IReadOnlyList<RoleElevationRequestDto> myRequests = [];
    private bool showHistory;
    private AdminActionGuard adminActionGuard = default!;

    private IEnumerable<RoleElevationRequestDto> VisibleRequests => showHistory
        ? myRequests
        : myRequests.Where(r => r.Status is RoleElevationStatus.Pending or RoleElevationStatus.Approved);

    protected override async Task OnInitializedAsync()
    {
        adminActionGuard = new AdminActionGuard(RateLimiter);
        await LoadRequestsAsync();
    }

    private async Task LoadRequestsAsync()
    {
        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        myRequests = userId is null
            ? []
            : await JitElevationService.GetRequestsForUserAsync(userId);
    }

    private async Task SubmitRequestAsync()
    {
        await adminActionGuard.RunAsync(AuthenticationStateTask, SubmitRequestCoreAsync);

        if (adminActionGuard.IsRateLimited)
        {
            errorMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
        }
    }

    private async Task SubmitRequestCoreAsync()
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

            await JitElevationService.RequestElevationAsync(userId, selectedRole, minutes, reason);

            reason = string.Empty;
            await LoadRequestsAsync();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            errorMessage = $"Error: {ex.Message}";
        }
    }

    private static string StatusBadgeClass(RoleElevationStatus status) => status switch
    {
        RoleElevationStatus.Pending => "badge-accent",
        RoleElevationStatus.Approved => "badge-primary",
        RoleElevationStatus.Rejected => "badge-danger-soft",
        RoleElevationStatus.Expired => "badge-archived",
        _ => string.Empty
    };

    private static string StatusLabel(RoleElevationStatus status) => status switch
    {
        RoleElevationStatus.Pending => "Afventer",
        RoleElevationStatus.Approved => "Godkendt",
        RoleElevationStatus.Rejected => "Afvist",
        RoleElevationStatus.Expired => "Udløbet",
        _ => status.ToString()
    };

    private static int ParseInt(object? value) =>
        int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var result) ? result : 0;
}
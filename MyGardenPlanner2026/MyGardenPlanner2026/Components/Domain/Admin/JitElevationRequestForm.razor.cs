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

    private const string UnexpectedFailureMessage =
    "Error: Handlingen kunne ikke gennemføres. Prøv igen, eller kontakt en administrator.";
    private const string LoadFailureMessage =
        "Error: Dine anmodninger kunne ikke indlæses. Prøv igen senere.";

    [Inject]
    private IJitElevationService JitElevationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [Inject]
    private ILogger<JitElevationRequestForm> Logger { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

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

    [LoggerMessage(EventId = 1078, Level = LogLevel.Information, Message = "JIT-anmodning om rollen '{RoleName}' afvist: {Reason}")]
    static partial void JitElevationRequestSubmitFailed(ILogger logger, string RoleName, string Reason);

    [LoggerMessage(EventId = 1139, Level = LogLevel.Error, Message = "Uventet fejl ved JIT-anmodning om rollen '{RoleName}'.")]
    static partial void JitElevationRequestUnexpectedFailure(ILogger logger, Exception ex, string RoleName);

    [LoggerMessage(EventId = 1140, Level = LogLevel.Error, Message = "Fejl ved indlæsning af brugerens JIT-anmodninger.")]
    static partial void JitElevationRequestsLoadFailed(ILogger logger, Exception ex);

    protected override Task OnInitializedAsync()
    {
        adminActionGuard = new AdminActionGuard(RateLimiter);
        return LoadRequestsAsync();
    }

    private async Task LoadRequestsAsync()
    {
        try
        {
            var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            myRequests = userId is null
                ? []
                : await JitElevationService.GetRequestsForUserAsync(userId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errorMessage = LoadFailureMessage;
            JitElevationRequestsLoadFailed(Logger, ex);
        }
    }

    private async Task SubmitRequestAsync()
    {
        // Ryd en gammel succesbesked på siden, før et nyt forsøg.
        await OnStatusMessage.InvokeAsync(string.Empty);
        await adminActionGuard.RunAsync(AuthenticationStateTask, SubmitRequestCoreAsync, message => errorMessage = message);
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

            var requestedRole = selectedRole;
            reason = string.Empty;
            await LoadRequestsAsync();
            await OnStatusMessage.InvokeAsync($"Anmodning om '{requestedRole}' er sendt.");
        }
        catch (Exception ex) when (ex is JitElevationValidationException or InvalidOperationException)
        {
            errorMessage = $"Error: {ex.Message}";
            JitElevationRequestSubmitFailed(Logger, selectedRole, ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errorMessage = UnexpectedFailureMessage;
            JitElevationRequestUnexpectedFailure(Logger, ex, selectedRole);
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
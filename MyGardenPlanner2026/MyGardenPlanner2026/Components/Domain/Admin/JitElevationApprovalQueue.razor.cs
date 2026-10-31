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
    private const string UnexpectedFailureMessage =
    "Error: Handlingen kunne ikke gennemføres. Prøv igen, eller kontakt en administrator.";
    private const string LoadFailureMessage =
        "Error: Anmodninger kunne ikke indlæses. Prøv igen senere.";

    [Inject]
    private IJitElevationService JitElevationService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [Inject]
    private ILogger<JitElevationApprovalQueue> Logger { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    private IReadOnlyList<RoleElevationRequestDto> pendingRequests = [];
    private string? errorMessage;
    private StepUpGuard stepUpGuard = default!;
    private AdminActionGuard adminActionGuard = default!;

    [LoggerMessage(EventId = 1079, Level = LogLevel.Information, Message = "Godkendelse af JIT-anmodning '{RequestId}' afvist: {Reason}")]
    static partial void JitElevationApprovalFailed(ILogger logger, Guid RequestId, string Reason);

    [LoggerMessage(EventId = 1080, Level = LogLevel.Information, Message = "Afvisning af JIT-anmodning '{RequestId}' afvist: {Reason}")]
    static partial void JitElevationRejectionFailed(ILogger logger, Guid RequestId, string Reason);

    [LoggerMessage(EventId = 1136, Level = LogLevel.Error, Message = "Uventet fejl ved godkendelse af JIT-anmodning '{RequestId}'.")]
    static partial void JitElevationApprovalUnexpectedFailure(ILogger logger, Exception ex, Guid RequestId);

    [LoggerMessage(EventId = 1137, Level = LogLevel.Error, Message = "Uventet fejl ved afvisning af JIT-anmodning '{RequestId}'.")]
    static partial void JitElevationRejectionUnexpectedFailure(ILogger logger, Exception ex, Guid RequestId);

    [LoggerMessage(EventId = 1138, Level = LogLevel.Error, Message = "Fejl ved indlæsning af JIT-anmodninger til godkendelse.")]
    static partial void JitElevationApprovalLoadFailed(ILogger logger, Exception ex);

    protected override Task OnInitializedAsync()
    {
        stepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        adminActionGuard = new AdminActionGuard(RateLimiter);
        return LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var approverId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            pendingRequests = approverId is null
                ? []
                : await JitElevationService.GetPendingRequestsForApprovalAsync(approverId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errorMessage = LoadFailureMessage;
            JitElevationApprovalLoadFailed(Logger, ex);
        }
    }

    private Task ApproveAsync(Guid requestId) =>
        stepUpGuard.RunAsync(AuthenticationStateTask, () =>
            adminActionGuard.RunAsync(
                AuthenticationStateTask, () => ApproveCoreAsync(requestId), message => errorMessage = message));

    private Task RejectAsync(Guid requestId) =>
        stepUpGuard.RunAsync(AuthenticationStateTask, () =>
            adminActionGuard.RunAsync(
                AuthenticationStateTask, () => RejectCoreAsync(requestId), message => errorMessage = message));

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
            JitElevationApprovalFailed(Logger, requestId, ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errorMessage = UnexpectedFailureMessage;
            JitElevationApprovalUnexpectedFailure(Logger, ex, requestId);
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
            JitElevationRejectionFailed(Logger, requestId, ex.Message);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            errorMessage = UnexpectedFailureMessage;
            JitElevationRejectionUnexpectedFailure(Logger, ex, requestId);
        }
    }
}
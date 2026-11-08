namespace MyGardenPlanner2026.Components.Domain.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;

/// <summary>
/// Fælles flow for sikkerhedspolicy-editors (§3.2/§4.1): indlæs → Gem → step-up (StepUpGuard)
/// → admin-rate limit (AdminActionGuard) → opdatering → genindlæsning → statusbesked.
/// Hver editor leverer kun sine felter, service-kaldet og beskeden.
/// </summary>
public abstract class PolicyEditorBase : ComponentBase
{
    [Inject]
    protected IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    protected IAdminActionRateLimiter RateLimiter { get; set; } = default!;

    [CascadingParameter]
    protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public EventCallback<string> OnStatusMessage { get; set; }

    protected string? ErrorMessage { get; private set; }

    protected StepUpGuard StepUpGuard { get; private set; } = default!;

    private AdminActionGuard adminActionGuard = default!;

    /// <summary>Succesbesked, der sendes til siden efter et vellykket gem.</summary>
    protected abstract string SavedMessage { get; }

    /// <summary>Henter policyen og nulstiller editorens redigeringsfelter.</summary>
    protected abstract Task LoadAsync();

    /// <summary>Gemmer editorens felter via den konkrete admin-service.</summary>
    protected abstract Task UpdateAsync(string userId);

    /// <summary>Logger en afvist opdatering (hver editor har eget EventId).</summary>
    protected abstract void LogSaveRejected(string reason);

    protected override Task OnInitializedAsync()
    {
        StepUpGuard = new StepUpGuard(AuthorizationService, AuthorizationServicesExtensions.RequireRecentAuthenticationPolicy);
        adminActionGuard = new AdminActionGuard(RateLimiter);
        return LoadAsync();
    }

    protected Task SaveAsync() =>
        StepUpGuard.RunAsync(AuthenticationStateTask, () =>
            adminActionGuard.RunAsync(AuthenticationStateTask, SaveCoreAsync, message => ErrorMessage = message));

    private async Task SaveCoreAsync()
    {
        ErrorMessage = null;
        try
        {
            var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
            if (userId is null)
            {
                ErrorMessage = AdminActionGuard.UserUnknownMessage;
                return;
            }

            await UpdateAsync(userId);

            await LoadAsync();
            await OnStatusMessage.InvokeAsync(SavedMessage);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            LogSaveRejected(ex.Message);
        }
    }

    protected static int ParseInt(object? value) =>
        int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var result) ? result : 0;
}
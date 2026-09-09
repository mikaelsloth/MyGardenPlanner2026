namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Interaktiv step-up re-autentificerings-modal. Forudsætter at forælderen allerede
/// kører i InteractiveServer (arver rendermode — sætter ikke selv @rendermode, jf.
/// static-to-interactive boundary-reglen). Bruger AuthenticationState (ikke HttpContext),
/// da HttpContext ikke er pålideligt tilgængeligt inde i en etableret circuit.
/// </summary>
public partial class StepUpReAuthModal
{
    [Inject]
    private UserManager<ApplicationUser> UserManager { get; set; } = default!;

    [Inject]
    private IReAuthenticationService ReAuthenticationService { get; set; } = default!;

    [Inject]
    private IReAuthFailureTracker ReAuthFailureTracker { get; set; } = default!;

    [Inject]
    private ICurrentUserAccessor CurrentUserAccessor { get; set; } = default!;

    [Inject]
    private ILogger<StepUpReAuthModal> Logger { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    [Parameter]
    public EventCallback OnReAuthenticated { get; set; }

    [LoggerMessage(EventId = 1035, Level = LogLevel.Warning, Message = "Forkert adgangskode angivet under step-up re-autentificering for bruger '{UserId}'.")]
    static partial void WrongPasswordAttempt(ILogger logger, string UserId);

    [LoggerMessage(EventId = 1036, Level = LogLevel.Warning, Message = "Ugyldig godkendelseskode angivet under step-up re-autentificering for bruger '{UserId}'.")]
    static partial void InvalidTwoFactorCodeAttempt(ILogger logger, string UserId);

    [LoggerMessage(EventId = 1037, Level = LogLevel.Information, Message = "Bruger '{UserId}' gennemførte step-up re-autentificering.")]
    static partial void ReAuthenticationSucceeded(ILogger logger, string UserId);

    [LoggerMessage(EventId = 1038, Level = LogLevel.Warning, Message = "Kunne ikke bestemme den aktuelle bruger under step-up re-autentificering.")]
    static partial void CurrentUserCouldNotBeResolved(ILogger logger);

    private InputModel Input { get; set; } = new();
    private ApplicationUser? currentUser;
    private string? errorMessage;
    private bool isVerifying;
    private bool requiresTwoFactor;
    private bool wasOpen;

    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && !wasOpen)
        {
            await LoadCurrentUserStateAsync();
        }
        else if (!IsOpen && wasOpen)
        {
            ResetForm();
        }

        wasOpen = IsOpen;
    }

    private async Task LoadCurrentUserStateAsync()
    {
        requiresTwoFactor = false;
        currentUser = null;

        if (AuthenticationStateTask is null)
        {
            return;
        }

        var authState = await AuthenticationStateTask;
        currentUser = await UserManager.GetUserAsync(authState.User);

        if (currentUser is not null)
        {
            requiresTwoFactor = await UserManager.GetTwoFactorEnabledAsync(currentUser);
        }
    }

    private async Task VerifyAsync()
    {
        errorMessage = null;

        if (currentUser is null)
        {
            errorMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";
            CurrentUserCouldNotBeResolved(Logger);
            return;
        }

        isVerifying = true;
        try
        {
            if (!await UserManager.CheckPasswordAsync(currentUser, Input.Password))
            {
                errorMessage = "Error: Forkert adgangskode.";
                WrongPasswordAttempt(Logger, currentUser.Id);
                await RecordFailureAsync();
                return;
            }

            if (requiresTwoFactor)
            {
                if (string.IsNullOrWhiteSpace(Input.TwoFactorCode))
                {
                    errorMessage = "Error: Indtast venligst din godkendelseskode.";
                    return;
                }

                var code = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
                var isCodeValid = await UserManager.VerifyTwoFactorTokenAsync(
                    currentUser, UserManager.Options.Tokens.AuthenticatorTokenProvider, code);

                if (!isCodeValid)
                {
                    errorMessage = "Error: Ugyldig godkendelseskode.";
                    InvalidTwoFactorCodeAttempt(Logger, currentUser.Id);
                    await RecordFailureAsync();
                    return;
                }
            }

            ReAuthenticationService.MarkReAuthenticated();
            await ReAuthFailureTracker.ClearFailuresAsync(currentUser.Id);
            ReAuthenticationSucceeded(Logger, currentUser.Id);
            ResetForm();
            await OnReAuthenticated.InvokeAsync();
        }
        finally
        {
            isVerifying = false;
        }
    }

    private async Task RecordFailureAsync()
    {
        if (currentUser is null)
        {
            return;
        }

        var ip = CurrentUserAccessor.GetCurrent().IpAddress;
        await ReAuthFailureTracker.RecordFailureAsync(currentUser.Id, ip);
    }

    private async Task CancelAsync()
    {
        ResetForm();
        await OnCancel.InvokeAsync();
    }

    private void ResetForm()
    {
        Input = new InputModel();
        errorMessage = null;
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Adgangskode er påkrævet.")]
        public string Password { get; set; } = "";

        public string? TwoFactorCode { get; set; }
    }
}
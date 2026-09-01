namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

public partial class LoginWithRecoveryCode
{
    private string? message;

    private ApplicationUser user = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [LoggerMessage(EventId = 1005, Level = LogLevel.Information, Message = "User with ID '{UserId}' logged in with a recovery code.")]
    static partial void UserLoggedIn(ILogger logger, string? UserId);

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = await SignInManager.GetTwoFactorAuthenticationUserAsync() ??
            throw new InvalidOperationException("Unable to load two-factor authentication user.");
    }

    private async Task OnValidSubmitAsync()
    {
        var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

        var result = await SignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        var userId = await UserManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            UserLoggedIn(Logger, userId);
            await ReAuthFailureTracker.ClearFailuresAsync(userId);
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else if (result.IsLockedOut)
        {
            Logger.LogWarning("User account locked out.");
            RedirectManager.RedirectTo("Account/Lockout");
        }
        else
        {
            var ip = CurrentUserAccessor.GetCurrent().IpAddress;
            await ReAuthFailureTracker.RecordFailureAsync(userId, ip);
            Logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", userId);
            message = "Error: Ugyldig gendannelseskode indtastet.";
        }
    }

    private sealed class InputModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = "";
    }
}

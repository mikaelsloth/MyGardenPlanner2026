namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities;
using System;

public partial class Disable2fa
{
    private ApplicationUser? user;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [LoggerMessage(EventId = 1016, Level = LogLevel.Information, Message = "User with ID '{UserId}' has disabled 2fa.")]
    static partial void TwoFactorDisabled(ILogger logger, string UserId);

    protected override async Task OnInitializedAsync()
    {
        user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        if (HttpMethods.IsGet(HttpContext.Request.Method) && !await UserManager.GetTwoFactorEnabledAsync(user))
        {
            throw new InvalidOperationException("Cannot disable 2FA for user as it's not currently enabled.");
        }
    }

    private async Task OnSubmitAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        var disable2faResult = await UserManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
        }

        var userId = await UserManager.GetUserIdAsync(user);
        TwoFactorDisabled(Logger, userId);
        RedirectManager.RedirectToWithStatus(
            "Account/Manage/TwoFactorAuthentication",
            "2fa has been disabled. You can reenable 2fa when you setup an authenticator app",
            HttpContext);
    }
}

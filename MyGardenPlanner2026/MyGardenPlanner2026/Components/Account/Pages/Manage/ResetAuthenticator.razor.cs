namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;

public partial class ResetAuthenticator
{
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [LoggerMessage(EventId = 1014, Level = LogLevel.Information, Message = "User with ID '{UserId}' has reset their authentication app key.")]
    static partial void AuthenticatorKeyReset(ILogger logger, string UserId);

    private async Task OnSubmitAsync()
    {
        var user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        await UserManager.SetTwoFactorEnabledAsync(user, false);
        await UserManager.ResetAuthenticatorKeyAsync(user);
        var userId = await UserManager.GetUserIdAsync(user);
        AuthenticatorKeyReset(Logger, userId);
        await SignInManager.RefreshSignInAsync(user);

        RedirectManager.RedirectToWithStatus(
            "Account/Manage/EnableAuthenticator",
            "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.",
            HttpContext);
    }
}

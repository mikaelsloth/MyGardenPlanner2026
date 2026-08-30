namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Components;

public partial class AccessDenied
{
    private bool requiresTwoFactorSetup;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            return;
        }

        requiresTwoFactorSetup = !await UserManager.GetTwoFactorEnabledAsync(user);

        if (requiresTwoFactorSetup)
        {
            RedirectManager.RedirectToWithStatus(
                "Account/Manage/EnableAuthenticator",
                "Adgang til denne funktion kræver, at du har totrinsbekræftelse aktiveret på din konto.",
                HttpContext);
        }
    }
}
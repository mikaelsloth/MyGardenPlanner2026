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
        if (user is not null)
        {
            requiresTwoFactorSetup = !await UserManager.GetTwoFactorEnabledAsync(user);
        }
    }
}
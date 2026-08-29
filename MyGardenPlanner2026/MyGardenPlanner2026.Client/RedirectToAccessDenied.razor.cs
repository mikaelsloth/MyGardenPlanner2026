namespace MyGardenPlanner2026.Client;

using Microsoft.AspNetCore.Components;

public partial class RedirectToAccessDenied
{
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo("Account/AccessDenied", forceLoad: true);
    }
}

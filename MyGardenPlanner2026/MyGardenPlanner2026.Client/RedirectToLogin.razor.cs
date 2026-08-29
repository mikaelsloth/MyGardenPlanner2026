namespace MyGardenPlanner2026.Client;

using Microsoft.AspNetCore.Components;
using System;

public partial class RedirectToLogin
{
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo($"Account/Login?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}", forceLoad: true);
    }
}

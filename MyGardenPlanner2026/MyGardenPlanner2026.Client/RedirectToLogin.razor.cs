namespace MyGardenPlanner2026.Client;

using Microsoft.AspNetCore.Components;
using System;

public partial class RedirectToLogin
{
    protected override void OnInitialized()
    {
        var returnUrl = new Uri(NavigationManager.Uri).PathAndQuery;
        NavigationManager.NavigateTo($"Account/Login?ReturnUrl={Uri.EscapeDataString(returnUrl)}", forceLoad: true);
    }
}

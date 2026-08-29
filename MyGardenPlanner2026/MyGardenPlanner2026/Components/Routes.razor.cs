namespace MyGardenPlanner2026.Components;

using Microsoft.AspNetCore.Components.Authorization;

public partial class Routes
{
    internal static bool IsAuthenticated(AuthenticationState authenticationState) =>
        authenticationState.User.Identity?.IsAuthenticated == true;
}
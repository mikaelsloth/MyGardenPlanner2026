namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>
/// Udtræk af e-mail-claim'et fra en cascaded AuthenticationState-task (supplerer
/// CurrentUserIdResolver). ASP.NET Identity sætter claim'et ved login, og login kræver en bekræftet
/// konto, så værdien er brugerens bekræftede e-mail. Returnerer null for ikke-autentificerede brugere.
/// </summary>
public static class CurrentUserEmailResolver
{
    public static async Task<string?> ResolveAsync(Task<AuthenticationState>? authenticationStateTask)
    {
        if (authenticationStateTask is null)
        {
            return null;
        }

        var user = (await authenticationStateTask).User;

        return user.Identity?.IsAuthenticated == true
            ? user.FindFirstValue(ClaimTypes.Email)
            : null;
    }
}
namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>
/// Fælles udtræk af NameIdentifier-claim'et fra en cascaded AuthenticationState-task.
/// Bruges af sikkerhedspolicy-editorerne til at stemple UpdatedByUserId på
/// AlertPolicyChangedAsync-kald, samt af AdminActionGuard til rate limiter-partitionering.
/// </summary>
public static class CurrentUserIdResolver
{
    public static async Task<string?> ResolveAsync(Task<AuthenticationState>? authenticationStateTask)
    {
        if (authenticationStateTask is null)
        {
            return null;
        }

        var authState = await authenticationStateTask;
        return authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
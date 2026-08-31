namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Security.Claims;

/// <summary>
/// Genanvendelig guard for "AdminApiPolicy" rate limiting (§4.1). Da admin-editors kalder
/// deres I*AdminService direkte via DI (intet HTTP API-lag), kan
/// Microsoft.AspNetCore.RateLimiting-middleware ikke ramme dem — denne klasse decorer'er
/// i stedet handlingen direkte, i samme stil som StepUpGuard.
///
/// Bevidst IKKE en Blazor-komponent eller DI-registreret service — ejes som felt af den
/// beskyttede komponent, oprettet i OnInitializedAsync.
/// </summary>
public sealed class AdminActionGuard(IAdminActionRateLimiter rateLimiter)
{
    /// <summary>True når det seneste RunAsync-kald blev afvist pga. rate limiting.</summary>
    public bool IsRateLimited { get; private set; }

    /// <summary>
    /// Udfører <paramref name="action"/>, hvis brugerens kvote ikke er opbrugt.
    /// Ellers sættes IsRateLimited til true, og handlingen udføres ikke.
    /// </summary>
    public async Task RunAsync(Task<AuthenticationState>? authenticationStateTask, Func<Task> action)
    {
        IsRateLimited = false;

        var userId = await ResolveUserIdAsync(authenticationStateTask);
        if (userId is null)
        {
            IsRateLimited = true;
            return;
        }

        if (!await rateLimiter.TryAcquireAsync(userId))
        {
            IsRateLimited = true;
            return;
        }

        await action();
    }

    private static async Task<string?> ResolveUserIdAsync(Task<AuthenticationState>? authenticationStateTask)
    {
        if (authenticationStateTask is null)
        {
            return null;
        }

        var authState = await authenticationStateTask;
        return authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
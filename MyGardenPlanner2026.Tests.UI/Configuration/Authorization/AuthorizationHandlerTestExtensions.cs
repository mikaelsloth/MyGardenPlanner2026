namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

/// <summary>
/// Fælles hjælper til at evaluere en authorization handler mod ét requirement.
/// </summary>
public static class AuthorizationHandlerTestExtensions
{
    /// <summary>
    /// Opretter en <see cref="AuthorizationHandlerContext"/>, kører handleren og returnerer,
    /// om requirementet blev opfyldt (<see cref="AuthorizationHandlerContext.HasSucceeded"/>).
    /// </summary>
    public static async Task<bool> EvaluateAsync(
        this IAuthorizationHandler handler,
        IAuthorizationRequirement requirement,
        ClaimsPrincipal user,
        object? resource = null)
    {
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await handler.HandleAsync(context).ConfigureAwait(false);

        return context.HasSucceeded;
    }
}
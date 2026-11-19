namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Bunit.TestDoubles;
using System.Security.Claims;

/// <summary>
/// Fælles opsætning af en autentificeret bruger i bUnit.
/// </summary>
public static class BunitAuthorizationExtensions
{
    /// <summary>
    /// Registrerer bUnit-autorisation og logger <paramref name="userId"/> ind.
    /// SetAuthorized(...) alene sætter kun ClaimTypes.Name, men CurrentUserIdResolver
    /// slår op på ClaimTypes.NameIdentifier, som derfor sættes eksplicit. CurrentUserEmailResolver
    /// slår op på ClaimTypes.Email, som kun sættes, hvis <paramref name="email"/> er angivet.
    /// </summary>
    /// <param name="userName">Værdi til ClaimTypes.Name. Udeladt bruges <paramref name="userId"/>.</param>
    /// <param name="email">Værdi til ClaimTypes.Email (brugerens bekræftede e-mail). Udeladt sættes claim'et ikke.</param>
    /// <returns>Autorisationskonteksten, så kaldere kan fortsætte med fx SetPolicies(...).</returns>
    public static BunitAuthorizationContext AuthorizeAs(
        this BunitContext context, string userId, string? userName = null, string? email = null)
    {
        var authContext = context.AddAuthorization();
        authContext.SetAuthorized(userName ?? userId);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (email is not null)
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        authContext.SetClaims([.. claims]);
        return authContext;
    }
}
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
    /// slår op på ClaimTypes.NameIdentifier, som derfor sættes eksplicit.
    /// </summary>
    /// <param name="userName">Værdi til ClaimTypes.Name. Udeladt bruges <paramref name="userId"/>.</param>
    /// <returns>Autorisationskonteksten, så kaldere kan fortsætte med fx SetPolicies(...).</returns>
    public static BunitAuthorizationContext AuthorizeAs(
        this BunitContext context, string userId, string? userName = null)
    {
        var authContext = context.AddAuthorization();
        authContext.SetAuthorized(userName ?? userId);
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
        return authContext;
    }
}
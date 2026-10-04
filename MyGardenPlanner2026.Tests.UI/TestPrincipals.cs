namespace MyGardenPlanner2026.Tests.UI;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>
/// Fælles factory til autentificerede og anonyme testbrugere (ClaimsPrincipal / AuthenticationState).
/// </summary>
public static class TestPrincipals
{
    public const string DefaultUserId = "user-1";

    private const string TestAuthenticationType = "TestAuth";

    /// <summary>
    /// Autentificeret bruger. <paramref name="userId"/> = null giver en autentificeret identitet
    /// uden NameIdentifier-claim.
    /// </summary>
    public static ClaimsPrincipal Create(string? userId = DefaultUserId, params string[] roles)
    {
        var claims = new List<Claim>();

        if (userId is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, TestAuthenticationType));
    }

    /// <summary>Ikke-autentificeret bruger (identitet uden authenticationType).</summary>
    public static ClaimsPrincipal Anonymous() => new(new ClaimsIdentity());

    public static Task<AuthenticationState> CreateAuthStateAsync(
        string? userId = DefaultUserId, params string[] roles) =>
        Task.FromResult(new AuthenticationState(Create(userId, roles)));
}
namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

/// <summary>
/// Centrale hjælpe-metoder til oprettelse af ClaimsPrincipal og AuthenticationState
/// i UI- og komponent-tests.
/// </summary>
public static class TestAuthHelper
{
    public const string DefaultUserId = "user-1";

    public static ClaimsPrincipal CreatePrincipal(
        string? userId = "user-1",
        string[]? roles = null,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            claims.Add(new Claim(ClaimTypes.Name, userId));
        }

        if (roles is not null)
        {
            foreach (var role in roles)
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        if (additionalClaims is not null)
        {
            claims.AddRange(additionalClaims);
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreatePrincipal(string? userId, params string[] roles) =>
        CreatePrincipal(userId, roles, additionalClaims: null);

    public static AuthenticationState CreateAuthState(ClaimsPrincipal principal) =>
        new(principal);

    public static AuthenticationState CreateAuthState(
        string? userId = "user-1",
        string[]? roles = null,
        IEnumerable<Claim>? additionalClaims = null) =>
        new(CreatePrincipal(userId, roles, additionalClaims));

    public static Task<AuthenticationState> CreateAuthStateAsync(ClaimsPrincipal principal) =>
        Task.FromResult(CreateAuthState(principal));

    public static Task<AuthenticationState> CreateAuthStateAsync(
        string? userId = "user-1",
        string[]? roles = null,
        IEnumerable<Claim>? additionalClaims = null) =>
        Task.FromResult(CreateAuthState(userId, roles, additionalClaims));

    public static Task<AuthenticationState> CreateAuthStateAsync(string? userId, params string[] roles) =>
        Task.FromResult(CreateAuthState(userId, roles, additionalClaims: null));

    // <summary>
    /// Registrerer og konfigurerer bUnits TestAuthorizationContext med NameIdentifier, Name og roller.
    /// Løser problemet med at bUnits standard SetAuthorized kun sætter ClaimTypes.Name.
    /// </summary>
    public static BunitAuthorizationContext AuthorizeUser(
        this BunitContext context,
        string userId = DefaultUserId,
        params string[] roles)
    {
        var authContext = context.AddAuthorization();
        authContext.SetAuthorized(userId);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        authContext.SetClaims([.. claims]);
        return authContext;
    }

    /// <summary>
    /// Hjælpemetode til hurtigt at autorisere en administratorbruger med en specifik admin-rolle.
    /// </summary>
    public static BunitAuthorizationContext AuthorizeAdmin(
        this BunitContext context,
        string userId = "admin-1",
        string role = "SystemAdmin") =>
        context.AuthorizeUser(userId, role);
}
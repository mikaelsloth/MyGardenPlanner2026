namespace MyGardenPlanner2026.Services;

using MyGardenPlanner2026.Core.Contracts.Common;
using System.Security.Claims;

/// <summary>
/// Ambient bruger for den igangværende Blazor-circuit-aktivitet. Sættes af CircuitUserCircuitHandler
/// og læses af HttpContextCurrentUserAccessor, når HttpContext er null (circuits). AsyncLocal gør, at
/// værdien følger den asynkrone kæde ned til EF Core-interceptors, uden at de skal kende circuit-scope.
/// </summary>
public static class CircuitUserContext
{
    private static readonly AsyncLocal<CurrentUserInfo?> CurrentValue = new();

    public static CurrentUserInfo? Current
    {
        get => CurrentValue.Value;
        set => CurrentValue.Value = value;
    }

    /// <summary>Null for en anonym bruger. IP-adressen er klientens IP fra circuit-forbindelsen, hvis kendt.</summary>
    public static CurrentUserInfo? From(ClaimsPrincipal? user, string? ipAddress = null)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity.Name;

        return userId is null && email is null ? null : new CurrentUserInfo(userId, email, ipAddress);
    }
}
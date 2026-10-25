namespace MyGardenPlanner2026.Services;

using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Core.Contracts.Common;
using System.Security.Claims;

/// <summary>
/// IHttpContextAccessor er selv registreret som singleton og læser HttpContext via
/// AsyncLocal, så denne klasse kan trygt injiceres i singleton EF Core-interceptors.
/// I en Blazor Server-circuit er HttpContext null: her bruges i stedet brugeren, som
/// CircuitUserCircuitHandler har sat i CircuitUserContext. IP-adressen kendes ikke i en circuit.
/// </summary>
public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUserInfo GetCurrent()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

        if (httpContext is not null)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = httpContext.User.FindFirstValue(ClaimTypes.Email) ?? httpContext.User.Identity?.Name;

            if (userId is not null || email is not null)
            {
                return new CurrentUserInfo(userId, email, ipAddress);
            }
        }

        return CircuitUserContext.Current is { } circuitUser
            ? circuitUser with { IpAddress = ipAddress }
            : new CurrentUserInfo(null, null, ipAddress);
    }
}
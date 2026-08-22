namespace MyGardenPlanner2026.Services;

using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Core.Contracts.Common;
using System.Security.Claims;

/// <summary>
/// IHttpContextAccessor er selv registreret som singleton og læser HttpContext via
/// AsyncLocal, så denne klasse kan trygt injiceres i singleton EF Core-interceptors.
/// </summary>
public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public CurrentUserInfo GetCurrent()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return new CurrentUserInfo(null, null, null);
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = httpContext.User.FindFirstValue(ClaimTypes.Email) ?? httpContext.User.Identity?.Name;
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        return new CurrentUserInfo(userId, email, ipAddress);
    }
}
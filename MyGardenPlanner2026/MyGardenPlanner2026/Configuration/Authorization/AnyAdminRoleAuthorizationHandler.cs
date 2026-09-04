namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Security.Claims;

/// <summary>
/// Godkender en AnyAdminRoleRequirement, hvis brugeren enten:
/// - er direkte medlem af mindst én af de angivne roller (claims-baseret, IsInRole), eller
/// - har en aktiv, godkendt JIT-eskalering til mindst én af dem (IJitElevationService).
/// Registreres som Scoped, da IJitElevationService er Scoped.
/// </summary>
public sealed class AnyAdminRoleAuthorizationHandler(IJitElevationService jitElevationService)
    : AuthorizationHandler<AnyAdminRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, AnyAdminRoleRequirement requirement)
    {
        foreach (var role in requirement.RoleNames)
        {
            if (context.User.IsInRole(role))
            {
                context.Succeed(requirement);
                return;
            }
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        foreach (var role in requirement.RoleNames)
        {
            if (await jitElevationService.HasActiveElevationAsync(userId, role))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
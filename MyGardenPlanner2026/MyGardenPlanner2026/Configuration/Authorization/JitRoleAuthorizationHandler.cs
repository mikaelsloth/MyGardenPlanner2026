namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Security.Claims;

/// <summary>
/// Godkender en JitRoleRequirement, hvis brugeren enten:
/// - er direkte medlem af den krævede rolle (claims-baseret, IsInRole), eller
/// - har en aktiv, godkendt JIT-eskalering til rollen (IJitElevationService).
/// Registreres som Scoped, da IJitElevationService er Scoped.
/// </summary>
public sealed class JitRoleAuthorizationHandler(IJitElevationService jitElevationService)
    : AuthorizationHandler<JitRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, JitRoleRequirement requirement)
    {
        if (context.User.IsInRole(requirement.RequiredRole))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        if (await jitElevationService.HasActiveElevationAsync(userId, requirement.RequiredRole))
        {
            context.Succeed(requirement);
        }
    }
}
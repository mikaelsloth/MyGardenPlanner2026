namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities;

/// <summary>
/// Godkender kun MfaRequirement, hvis brugeren har TwoFactorEnabled == true.
/// Bruges sammen med JitRoleRequirement på admin-policies, så adgang kræver
/// (rolle ELLER aktiv JIT) OG 2FA.
/// </summary>
public sealed class MfaAuthorizationHandler(UserManager<ApplicationUser> userManager)
    : AuthorizationHandler<MfaRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, MfaRequirement requirement)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user is null)
        {
            return;
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            context.Succeed(requirement);
        }
    }
}
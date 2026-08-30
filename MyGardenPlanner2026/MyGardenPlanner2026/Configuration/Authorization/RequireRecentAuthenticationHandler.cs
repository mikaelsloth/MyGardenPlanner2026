namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Godkender kun RequireRecentAuthenticationRequirement, hvis brugerens seneste
/// step-up re-autentificering (se IReAuthenticationService) er yngre end den
/// konfigurerede MaxAgeMinutes. Registreres Scoped, da IReAuthenticationService er Scoped.
/// </summary>
public sealed class RequireRecentAuthenticationHandler(
    IReAuthenticationService reAuthenticationService,
    IOptions<ReAuthenticationPolicyOptions> policyOptions)
    : AuthorizationHandler<RequireRecentAuthenticationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, RequireRecentAuthenticationRequirement requirement)
    {
        var maxAge = TimeSpan.FromMinutes(policyOptions.Value.MaxAgeMinutes);

        if (reAuthenticationService.IsReAuthValid(maxAge))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
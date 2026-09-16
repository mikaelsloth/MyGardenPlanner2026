namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using System.Security.Claims;

/// <summary>
/// Godkender en GardenMemberRequirement, hvis brugeren har et GardenMembership for den
/// have (Guid-ressource) der angives ved AuthorizeAsync-kaldet — se
/// GardenAccessManagementPage.OnInitializedAsync, som kalder AuthorizeAsync med GardenId
/// som ressource, da et route-parameter ikke kan bindes til en almindelig attribute-policy.
/// </summary>
public sealed class GardenMemberAuthorizationHandler(IGardenAccessQueryService queryService)
    : AuthorizationHandler<GardenMemberRequirement, Guid>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, GardenMemberRequirement requirement, Guid gardenId)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var membership = await queryService.GetMembershipAsync(gardenId, userId);
        if (membership is not null)
        {
            context.Succeed(requirement);
        }
    }
}
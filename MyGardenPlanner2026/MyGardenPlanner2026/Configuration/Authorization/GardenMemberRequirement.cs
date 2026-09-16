namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Kræver at brugeren har et aktivt GardenMembership for den have (Guid-ressource) der
/// angives ved AuthorizeAsync-kaldet. Se GardenMemberAuthorizationHandler.
/// </summary>
public sealed record GardenMemberRequirement : IAuthorizationRequirement;
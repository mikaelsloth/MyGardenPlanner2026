namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;

public sealed record MfaRequirement : IAuthorizationRequirement;
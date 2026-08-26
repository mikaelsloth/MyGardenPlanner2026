namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Kræver enten den angivne Identity-rolle direkte, eller en aktiv, godkendt
/// JIT-eskalering til samme rolle (se JitRoleAuthorizationHandler).
/// </summary>
public sealed record JitRoleRequirement(string RequiredRole) : IAuthorizationRequirement;
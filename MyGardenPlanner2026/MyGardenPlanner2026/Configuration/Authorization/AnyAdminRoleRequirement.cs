namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Kræver, at brugeren enten direkte har mindst én af de angivne roller, eller har en
/// aktiv, godkendt JIT-eskalering til mindst én af dem (se AnyAdminRoleAuthorizationHandler).
/// Bruges til at afgrænse adgang til admin-området generelt, fx JIT-anmodningssiden.
/// </summary>
public sealed record AnyAdminRoleRequirement(IReadOnlyList<string> RoleNames) : IAuthorizationRequirement;
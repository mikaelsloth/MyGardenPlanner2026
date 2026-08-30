namespace MyGardenPlanner2026.Configuration.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Kræver, at brugeren har gennemført step-up re-autentificering (adgangskode/TOTP)
/// inden for det tidsrum, der er konfigureret i ReAuthenticationPolicyOptions.
/// Se RequireRecentAuthenticationHandler for selve tjekket.
/// </summary>
public sealed record RequireRecentAuthenticationRequirement : IAuthorizationRequirement;
namespace MyGardenPlanner2026.Tests.E2E.Infrastructure;

/// <summary>
/// Rå testdata for én smoke-test-bruger. AuthenticatorKey er den uformaterede Base32-nøgle
/// (samme værdi som UserManager.GetAuthenticatorKeyAsync returnerer) — bruges i senere
/// PR'er til at udregne et gyldigt TOTP-login-kode programmatisk uden UI-interaktion.
/// </summary>
public sealed record SmokeTestUser(
    string Email,
    string Password,
    string? Role,
    bool TwoFactorEnabled,
    string? AuthenticatorKey);
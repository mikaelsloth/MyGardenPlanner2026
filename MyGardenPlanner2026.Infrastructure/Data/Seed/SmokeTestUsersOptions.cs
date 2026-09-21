namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

/// <summary>
/// Konfiguration for smoke-test-brugere (kun Debug/Development). Læses fra user-secrets
/// under sektionen "SmokeTestUsers" — credentials hører ALDRIG i appsettings.json.
/// </summary>
public sealed class SmokeTestUsersOptions
{
    public const string SectionName = "SmokeTestUsers";

    /// <summary>Fælles adgangskode til alle smoke-test-brugere (kun brugt ved oprettelse).</summary>
    public string? Password { get; set; }

    /// <summary>Base32-kodet TOTP-hemmelighed, delt af alle 2FA-brugere.</summary>
    public string? AuthenticatorKey { get; set; }
}
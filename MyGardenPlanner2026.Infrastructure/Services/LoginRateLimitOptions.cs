namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfigurerbare grænser for login-endpoint rate limiteren (§4.1). Bundet fra
/// appsettings under sektionen "LoginRateLimit". Bruges i PR1 udelukkende som
/// day-0-default-kilde til seederen — selve forbruget (GlobalLimiter) migreres
/// fra hardkodede værdier i en senere PR.
/// </summary>
public sealed class LoginRateLimitOptions
{
    public const string SectionName = "LoginRateLimit";

    public int PermitLimit { get; set; } = 5;
    public int WindowSeconds { get; set; } = 60;
}
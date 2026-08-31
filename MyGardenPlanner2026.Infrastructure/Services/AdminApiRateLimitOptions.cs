namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfigurerbare grænser for "AdminApiPolicy" (§4.1), bundet fra appsettings under
/// sektionen "AdminApiRateLimit".
/// </summary>
public sealed class AdminApiRateLimitOptions
{
    public const string SectionName = "AdminApiRateLimit";

    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int SegmentsPerWindow { get; set; } = 6;
}
namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfigurerbare grænser for tracking af fejlede MFA/re-auth-forsøg (§4.2), bundet fra
/// appsettings under sektionen "ReAuthFailureTracking".
/// </summary>
public sealed class ReAuthFailureTrackerOptions
{
    public const string SectionName = "ReAuthFailureTracking";

    public int Threshold { get; set; } = 5;
    public int WindowDays { get; set; } = 2;
}
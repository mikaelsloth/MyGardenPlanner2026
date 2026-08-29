namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfigurerbare grænser for JIT-elevationsvarighed og sweep-interval, bundet fra
/// appsettings under sektionen "JitElevationPolicy".
/// </summary>
public sealed class JitElevationPolicyOptions
{
    public const string SectionName = "JitElevationPolicy";

    public int MinRequestedMinutes { get; set; } = 30;
    public int MaxRequestedMinutes { get; set; } = 90;
    public int SweepIntervalMinutes { get; set; } = 5;
}
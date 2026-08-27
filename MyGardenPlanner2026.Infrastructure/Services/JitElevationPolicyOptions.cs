namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfigurerbare grænser for JIT-elevationsvarighed, bundet fra appsettings
/// under sektionen "JitElevationPolicy". Gør det muligt at justere grænserne
/// uden kodeændring/deployment.
/// </summary>
public sealed class JitElevationPolicyOptions
{
    public const string SectionName = "JitElevationPolicy";

    public int MinRequestedMinutes { get; set; } = 30;
    public int MaxRequestedMinutes { get; set; } = 90;
}
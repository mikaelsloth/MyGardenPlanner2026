namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Runtime-styret modstykke til JitElevationPolicyOptions (appsettings-sektionen
/// "JitElevationPolicy" bruges kun som day-0 default ved seeding, se
/// SecurityPolicySettingsSeeder). Singleton-række — Id er altid SingletonId.
/// </summary>
public class JitElevationPolicySettings : ISoftDelete, ISingletonSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public Guid Id { get; init; } = SingletonId;

    public int MinRequestedMinutes { get; set; }
    public int MaxRequestedMinutes { get; set; }
    public int SweepIntervalMinutes { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
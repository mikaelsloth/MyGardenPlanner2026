namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>Runtime-styret modstykke til ReAuthFailureTrackerOptions ("ReAuthFailureTracking").</summary>
public class ReAuthFailureTrackerSettings : ISoftDelete, ISingletonSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    public Guid Id { get; init; } = SingletonId;

    public int Threshold { get; set; }
    public int WindowDays { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
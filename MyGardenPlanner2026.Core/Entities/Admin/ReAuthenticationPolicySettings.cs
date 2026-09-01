namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>Runtime-styret modstykke til ReAuthenticationPolicyOptions ("ReAuthenticationPolicy").</summary>
public class ReAuthenticationPolicySettings : ISoftDelete, ISingletonSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public Guid Id { get; init; } = SingletonId;

    public int MaxAgeMinutes { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
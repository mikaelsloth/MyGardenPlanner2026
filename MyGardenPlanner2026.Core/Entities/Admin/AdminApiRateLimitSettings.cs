namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>Runtime-styret modstykke til AdminApiRateLimitOptions ("AdminApiRateLimit").</summary>
public class AdminApiRateLimitSettings : ISoftDelete, ISingletonSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public Guid Id { get; init; } = SingletonId;

    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
    public int SegmentsPerWindow { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
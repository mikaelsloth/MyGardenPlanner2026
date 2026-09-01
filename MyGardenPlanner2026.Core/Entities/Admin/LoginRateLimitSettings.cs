namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Runtime-styret modstykke til LoginRateLimitOptions ("LoginRateLimit"). Erstatter de
/// hidtil hardkodede værdier (5 forsøg / 60 sek.) på login-endpointsne. Selve forbruget
/// (reloadable GlobalLimiter) migreres i en senere PR.
/// </summary>
public class LoginRateLimitSettings : ISoftDelete, ISingletonSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000005");

    public Guid Id { get; init; } = SingletonId;

    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
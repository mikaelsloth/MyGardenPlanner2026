namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Runtime-styret modstykke til AuditLogExportJobOptions ("AuditLogExportJob") — kun
/// RetentionHours og MaxActiveJobsPerUser er DB-styret; de øvrige felter (poll-interval,
/// stale-timeout, filstørrelsesgrænse) forbliver rene appsettings-defaults.
/// </summary>
public class AuditLogExportJobPolicySettings : ISoftDelete, ISingletonSettings
{
    public static readonly Guid SingletonId = Guid.Parse("00000000-0000-0000-0000-000000000006");

    public Guid Id { get; init; } = SingletonId;

    public int RetentionHours { get; set; }
    public int MaxActiveJobsPerUser { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
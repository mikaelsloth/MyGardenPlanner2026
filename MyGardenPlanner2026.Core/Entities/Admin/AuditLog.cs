namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Append-only revisionslog. Må ALDRIG opdateres eller slettes fra applikationskoden —
/// databasebrugerne gives kun INSERT-rettighed på denne tabel (se Database-scripts 03).
/// Implementerer bevidst ikke ISoftDelete og bruger ikke Temporal Tables — loggen er
/// i sig selv den historiske sandhed og skal ikke kunne ændres, heller ikke "blødt".
/// </summary>
public class AuditLog
{
    public long Id { get; set; }

    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }

    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
}
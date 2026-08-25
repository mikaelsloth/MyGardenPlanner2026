namespace MyGardenPlanner2026.Core.Entities.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// JIT-anmodning om midlertidig rolle-eskalering. Peer-godkendelse (ApproverUserId !=
/// RequesterUserId) håndhæves i IJitElevationService (PR2), ikke i datalaget.
/// Implementerer ISoftDelete udelukkende for at blive fanget af AuditLoggingInterceptor —
/// anmodninger slettes i praksis aldrig, kun status-ændres.
/// </summary>
public class RoleElevationRequest : ISoftDelete
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public string RequesterUserId { get; set; } = string.Empty;
    public string? ApproverUserId { get; set; }

    public string RoleName { get; set; } = string.Empty;
    public RoleElevationStatus Status { get; set; } = RoleElevationStatus.Pending;

    public string Reason { get; set; } = string.Empty;
    public int RequestedHours { get; set; }

    public DateTimeOffset? ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
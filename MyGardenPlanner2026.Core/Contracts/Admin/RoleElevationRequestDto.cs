namespace MyGardenPlanner2026.Core.Contracts.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

public sealed record RoleElevationRequestDto(
    Guid Id,
    string RequesterUserId,
    string? ApproverUserId,
    string RoleName,
    RoleElevationStatus Status,
    string Reason,
    int RequestedHours,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidToUtc,
    DateTimeOffset CreatedAtUtc);
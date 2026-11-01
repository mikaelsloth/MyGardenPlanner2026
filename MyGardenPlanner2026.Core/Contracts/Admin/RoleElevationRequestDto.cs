namespace MyGardenPlanner2026.Core.Contracts.Admin;

using MyGardenPlanner2026.Core.Entities.Common;

/// <param name="RequesterEmail">
/// Ansøgerens e-mail. Udfyldes kun af GetPendingRequestsForApprovalAsync; null = brugeren findes ikke
/// længere (slettet). Øvrige metoder returnerer null.
/// </param>
public sealed record RoleElevationRequestDto(
    Guid Id,
    string RequesterUserId,
    string? ApproverUserId,
    string RoleName,
    RoleElevationStatus Status,
    string Reason,
    int RequestedMinutes,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidToUtc,
    DateTimeOffset CreatedAtUtc,
    string? RequesterEmail = null);
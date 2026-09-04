namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IJitElevationService
{
    Task<RoleElevationRequestDto> RequestElevationAsync(
        string userId, string roleName, int minutes, string reason, CancellationToken cancellationToken = default);

    Task<RoleElevationRequestDto> ApproveElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default);

    Task<RoleElevationRequestDto> RejectElevationAsync(
        string approverUserId, Guid requestId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveElevationAsync(
        string userId, string roleName, CancellationToken cancellationToken = default);

    /// <summary>Alle anmodninger (uanset status) indsendt af den angivne bruger, nyeste først.</summary>
    Task<IReadOnlyList<RoleElevationRequestDto>> GetRequestsForUserAsync(
        string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ventende (Pending) anmodninger som <paramref name="approverUserId"/> kan godkende —
    /// dvs. brugeren er direkte medlem af den anmodede rolle, og er ikke selv ansøgeren.
    /// </summary>
    Task<IReadOnlyList<RoleElevationRequestDto>> GetPendingRequestsForApprovalAsync(
        string approverUserId, CancellationToken cancellationToken = default);
}
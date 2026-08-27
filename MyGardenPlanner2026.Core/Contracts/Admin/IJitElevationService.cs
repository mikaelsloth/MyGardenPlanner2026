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
}
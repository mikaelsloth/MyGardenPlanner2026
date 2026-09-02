namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IJitElevationPolicyAdminService
{
    Task<JitElevationPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<JitElevationPolicyDto> UpdateAsync(
        JitElevationPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}
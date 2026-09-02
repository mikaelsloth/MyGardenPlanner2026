namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IReAuthFailureTrackerPolicyAdminService
{
    Task<ReAuthFailureTrackerPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<ReAuthFailureTrackerPolicyDto> UpdateAsync(
        ReAuthFailureTrackerPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}
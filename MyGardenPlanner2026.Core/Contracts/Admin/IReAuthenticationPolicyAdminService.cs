namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IReAuthenticationPolicyAdminService
{
    Task<ReAuthenticationPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<ReAuthenticationPolicyDto> UpdateAsync(
        ReAuthenticationPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}
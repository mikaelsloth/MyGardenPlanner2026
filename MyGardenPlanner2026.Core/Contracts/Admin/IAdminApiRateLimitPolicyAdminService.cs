namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IAdminApiRateLimitPolicyAdminService
{
    Task<AdminApiRateLimitPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<AdminApiRateLimitPolicyDto> UpdateAsync(
        AdminApiRateLimitPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}
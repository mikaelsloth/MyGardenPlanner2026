namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface ILoginRateLimitPolicyAdminService
{
    Task<LoginRateLimitPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<LoginRateLimitPolicyDto> UpdateAsync(
        LoginRateLimitPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}
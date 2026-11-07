namespace MyGardenPlanner2026.Core.Contracts.Admin;

public interface IReAuthenticationPolicyAdminService
{
    /// <summary>Højeste tilladte værdi for MaxAgeMinutes (minutter).</summary>
    public const int MaxAllowedAgeMinutes = 60;

    Task<ReAuthenticationPolicyDto> GetAsync(CancellationToken cancellationToken = default);

    Task<ReAuthenticationPolicyDto> UpdateAsync(
        ReAuthenticationPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default);
}
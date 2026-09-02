namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Læser/opdaterer den runtime-styrede step-up re-autentificerings-policy (§3.2).</summary>
public sealed class ReAuthenticationPolicyAdminService(
    IAdminDbContextFactory contextFactory,
    ISecurityPolicyChangeSignal changeSignal,
    ISecurityAlertService securityAlertService) : IReAuthenticationPolicyAdminService
{
    public async Task<ReAuthenticationPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return ToDto(await LoadAsync(context, cancellationToken));
    }

    public async Task<ReAuthenticationPolicyDto> UpdateAsync(
        ReAuthenticationPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedByUserId);

        if (update.MaxAgeMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "MaxAgeMinutes skal være positiv.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await LoadAsync(context, cancellationToken);

        settings.MaxAgeMinutes = update.MaxAgeMinutes;
        await context.SaveChangesAsync(cancellationToken);

        changeSignal.TriggerChange<ReAuthenticationPolicyOptions>();
        await securityAlertService.AlertPolicyChangedAsync(updatedByUserId, "ReAuthenticationPolicy", cancellationToken);

        return ToDto(settings);
    }

    private static async Task<ReAuthenticationPolicySettings> LoadAsync(
        PlannerDbContext context, CancellationToken cancellationToken) =>
        await context.ReAuthenticationPolicySettings
            .SingleOrDefaultAsync(s => s.Id == ReAuthenticationPolicySettings.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("Re-autentificeringspolicy er ikke seedet.");

    private static ReAuthenticationPolicyDto ToDto(ReAuthenticationPolicySettings settings) => new(settings.MaxAgeMinutes);
}
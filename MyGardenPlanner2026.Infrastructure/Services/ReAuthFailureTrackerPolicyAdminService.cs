namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Læser/opdaterer den runtime-styrede fejl-tracker-policy for MFA/re-auth-forsøg (§4.2).</summary>
public sealed class ReAuthFailureTrackerPolicyAdminService(
    IAdminDbContextFactory contextFactory,
    ISecurityPolicyChangeSignal changeSignal,
    ISecurityAlertService securityAlertService) : IReAuthFailureTrackerPolicyAdminService
{
    public async Task<ReAuthFailureTrackerPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return ToDto(await LoadAsync(context, cancellationToken));
    }

    public async Task<ReAuthFailureTrackerPolicyDto> UpdateAsync(
        ReAuthFailureTrackerPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedByUserId);

        if (update.Threshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "Threshold skal være positiv.");
        }

        if (update.WindowDays < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "WindowDays skal være positiv.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await LoadAsync(context, cancellationToken);

        settings.Threshold = update.Threshold;
        settings.WindowDays = update.WindowDays;
        await context.SaveChangesAsync(cancellationToken);

        changeSignal.TriggerChange<ReAuthFailureTrackerOptions>();
        await securityAlertService.AlertPolicyChangedAsync(updatedByUserId, "ReAuthFailureTrackerPolicy", cancellationToken);

        return ToDto(settings);
    }

    private static async Task<ReAuthFailureTrackerSettings> LoadAsync(
        PlannerDbContext context, CancellationToken cancellationToken) =>
        await context.ReAuthFailureTrackerSettings
            .SingleOrDefaultAsync(s => s.Id == ReAuthFailureTrackerSettings.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("Fejl-tracker-policy er ikke seedet.");

    private static ReAuthFailureTrackerPolicyDto ToDto(ReAuthFailureTrackerSettings settings) =>
        new(settings.Threshold, settings.WindowDays);
}
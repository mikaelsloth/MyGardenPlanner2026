namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Læser/opdaterer den runtime-styrede JIT-eskalerings-policy (§3.2). Efter et succesfuldt
/// save udløses ISecurityPolicyChangeSignal, så JitElevationService (IOptionsMonitor)
/// reflekterer ændringen på næste kald — UDEN proces-genstart (fra PR3). Sender desuden
/// en sikkerhedsalarm via ISecurityAlertService.
/// </summary>
public sealed class JitElevationPolicyAdminService(
    IAdminDbContextFactory contextFactory,
    ISecurityPolicyChangeSignal changeSignal,
    ISecurityAlertService securityAlertService) : IJitElevationPolicyAdminService
{
    public async Task<JitElevationPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return ToDto(await LoadAsync(context, cancellationToken));
    }

    public async Task<JitElevationPolicyDto> UpdateAsync(
        JitElevationPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedByUserId);

        if (update.MinRequestedMinutes < 1 || update.MaxRequestedMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "Minutter skal være positive.");
        }

        if (update.MinRequestedMinutes > update.MaxRequestedMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(update), "MinRequestedMinutes kan ikke være større end MaxRequestedMinutes.");
        }

        if (update.SweepIntervalMinutes < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "SweepIntervalMinutes skal være positiv.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await LoadAsync(context, cancellationToken);

        settings.MinRequestedMinutes = update.MinRequestedMinutes;
        settings.MaxRequestedMinutes = update.MaxRequestedMinutes;
        settings.SweepIntervalMinutes = update.SweepIntervalMinutes;

        await context.SaveChangesAsync(cancellationToken);

        changeSignal.TriggerChange<JitElevationPolicyOptions>();
        await securityAlertService.AlertPolicyChangedAsync(updatedByUserId, "JitElevationPolicy", cancellationToken);

        return ToDto(settings);
    }

    private static async Task<JitElevationPolicySettings> LoadAsync(
        PlannerDbContext context, CancellationToken cancellationToken) =>
        await context.JitElevationPolicySettings
            .SingleOrDefaultAsync(s => s.Id == JitElevationPolicySettings.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("JIT-eskaleringspolicy er ikke seedet.");

    private static JitElevationPolicyDto ToDto(JitElevationPolicySettings settings) =>
        new(settings.MinRequestedMinutes, settings.MaxRequestedMinutes, settings.SweepIntervalMinutes);
}
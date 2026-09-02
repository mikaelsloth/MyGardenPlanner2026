namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Læser/opdaterer den runtime-styrede rate limit-policy for admin CRUD-handlinger (§4.1).</summary>
public sealed class AdminApiRateLimitPolicyAdminService(
    IAdminDbContextFactory contextFactory,
    ISecurityPolicyChangeSignal changeSignal,
    ISecurityAlertService securityAlertService) : IAdminApiRateLimitPolicyAdminService
{
    public async Task<AdminApiRateLimitPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return ToDto(await LoadAsync(context, cancellationToken));
    }

    public async Task<AdminApiRateLimitPolicyDto> UpdateAsync(
        AdminApiRateLimitPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedByUserId);

        if (update.PermitLimit < 1 || update.WindowSeconds < 1 || update.SegmentsPerWindow < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(update), "PermitLimit, WindowSeconds og SegmentsPerWindow skal være positive.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await LoadAsync(context, cancellationToken);

        settings.PermitLimit = update.PermitLimit;
        settings.WindowSeconds = update.WindowSeconds;
        settings.SegmentsPerWindow = update.SegmentsPerWindow;
        await context.SaveChangesAsync(cancellationToken);

        changeSignal.TriggerChange<AdminApiRateLimitOptions>();
        await securityAlertService.AlertPolicyChangedAsync(updatedByUserId, "AdminApiRateLimitPolicy", cancellationToken);

        return ToDto(settings);
    }

    private static async Task<AdminApiRateLimitSettings> LoadAsync(
        PlannerDbContext context, CancellationToken cancellationToken) =>
        await context.AdminApiRateLimitSettings
            .SingleOrDefaultAsync(s => s.Id == AdminApiRateLimitSettings.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("Admin-API rate limit-policy er ikke seedet.");

    private static AdminApiRateLimitPolicyDto ToDto(AdminApiRateLimitSettings settings) =>
        new(settings.PermitLimit, settings.WindowSeconds, settings.SegmentsPerWindow);
}
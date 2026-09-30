namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Læser/opdaterer den runtime-styrede retention-/maks.-aktive-policy for AuditLog-baggrundseksporter.</summary>
public sealed class AuditLogExportJobPolicyAdminService(
    IAdminDbContextFactory contextFactory,
    ISecurityPolicyChangeSignal changeSignal,
    ISecurityAlertService securityAlertService) : IAuditLogExportJobPolicyAdminService
{
    public async Task<AuditLogExportJobPolicyDto> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return ToDto(await LoadAsync(context, cancellationToken));
    }

    public async Task<AuditLogExportJobPolicyDto> UpdateAsync(
        AuditLogExportJobPolicyDto update, string updatedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedByUserId);

        if (update.RetentionHours < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "RetentionHours skal være mindst 1.");
        }

        if (update.MaxActiveJobsPerUser < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(update), "MaxActiveJobsPerUser skal være mindst 1.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await LoadAsync(context, cancellationToken);

        settings.RetentionHours = update.RetentionHours;
        settings.MaxActiveJobsPerUser = update.MaxActiveJobsPerUser;
        await context.SaveChangesAsync(cancellationToken);

        changeSignal.TriggerChange<AuditLogExportJobOptions>();
        await securityAlertService.AlertPolicyChangedAsync(updatedByUserId, "AuditLogExportJobPolicy", cancellationToken);

        return ToDto(settings);
    }

    private static async Task<AuditLogExportJobPolicySettings> LoadAsync(
        PlannerDbContext context, CancellationToken cancellationToken) =>
        await context.AuditLogExportJobPolicySettings
            .SingleOrDefaultAsync(s => s.Id == AuditLogExportJobPolicySettings.SingletonId, cancellationToken)
            ?? throw new InvalidOperationException("AuditLog-eksportjob-policy er ikke seedet.");

    private static AuditLogExportJobPolicyDto ToDto(AuditLogExportJobPolicySettings settings) =>
        new(settings.RetentionHours, settings.MaxActiveJobsPerUser);
}
namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Se JitElevationPolicyOptionsConfigurator for det fælles registrerings-/fallback-mønster.
/// Overskriver kun RetentionHours og MaxActiveJobsPerUser — øvrige AuditLogExportJobOptions-
/// felter er fortsat rene appsettings-defaults.
/// </summary>
public sealed class AuditLogExportJobOptionsConfigurator(IAdminDbContextFactory contextFactory)
    : IConfigureOptions<AuditLogExportJobOptions>
{
    public void Configure(AuditLogExportJobOptions options)
    {
        using var context = contextFactory.CreateDbContext();
        var settings = context.AuditLogExportJobPolicySettings.Find(AuditLogExportJobPolicySettings.SingletonId);

        if (settings is null)
        {
            return;
        }

        options.RetentionHours = settings.RetentionHours;
        options.MaxActiveJobsPerUser = settings.MaxActiveJobsPerUser;
    }
}
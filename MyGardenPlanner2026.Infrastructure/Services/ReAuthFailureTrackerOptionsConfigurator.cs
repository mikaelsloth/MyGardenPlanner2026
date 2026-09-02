namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Se JitElevationPolicyOptionsConfigurator for det fælles registrerings-/fallback-mønster.</summary>
public sealed class ReAuthFailureTrackerOptionsConfigurator(IAdminDbContextFactory contextFactory)
    : IConfigureOptions<ReAuthFailureTrackerOptions>
{
    public void Configure(ReAuthFailureTrackerOptions options)
    {
        using var context = contextFactory.CreateDbContext();
        var settings = context.ReAuthFailureTrackerSettings.Find(ReAuthFailureTrackerSettings.SingletonId);

        if (settings is null)
        {
            return;
        }

        options.Threshold = settings.Threshold;
        options.WindowDays = settings.WindowDays;
    }
}
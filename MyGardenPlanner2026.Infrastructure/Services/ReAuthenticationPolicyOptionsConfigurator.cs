namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>Se JitElevationPolicyOptionsConfigurator for det fælles registrerings-/fallback-mønster.</summary>
public sealed class ReAuthenticationPolicyOptionsConfigurator(IAdminDbContextFactory contextFactory)
    : IConfigureOptions<ReAuthenticationPolicyOptions>
{
    public void Configure(ReAuthenticationPolicyOptions options)
    {
        using var context = contextFactory.CreateDbContext();
        var settings = context.ReAuthenticationPolicySettings.Find(ReAuthenticationPolicySettings.SingletonId);

        if (settings is null)
        {
            return;
        }

        options.MaxAgeMinutes = settings.MaxAgeMinutes;
    }
}
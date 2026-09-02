namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Se JitElevationPolicyOptionsConfigurator for det fælles registrerings-/fallback-mønster.
/// OBS: AdminActionRateLimiter (Singleton) bygger stadig sin PartitionedRateLimiter én
/// gang ved opstart indtil PR3 — se PR-note.
/// </summary>
public sealed class AdminApiRateLimitOptionsConfigurator(IAdminDbContextFactory contextFactory)
    : IConfigureOptions<AdminApiRateLimitOptions>
{
    public void Configure(AdminApiRateLimitOptions options)
    {
        using var context = contextFactory.CreateDbContext();
        var settings = context.AdminApiRateLimitSettings.Find(AdminApiRateLimitSettings.SingletonId);

        if (settings is null)
        {
            return;
        }

        options.PermitLimit = settings.PermitLimit;
        options.WindowSeconds = settings.WindowSeconds;
        options.SegmentsPerWindow = settings.SegmentsPerWindow;
    }
}
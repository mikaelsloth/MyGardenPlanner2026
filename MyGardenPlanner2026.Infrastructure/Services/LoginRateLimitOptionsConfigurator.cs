namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Se JitElevationPolicyOptionsConfigurator for det fælles registrerings-/fallback-mønster.
/// OBS: selve login-GlobalLimiter-forbruget er stadig hardkodet indtil PR3.
/// </summary>
public sealed class LoginRateLimitOptionsConfigurator(IAdminDbContextFactory contextFactory)
    : IConfigureOptions<LoginRateLimitOptions>
{
    public void Configure(LoginRateLimitOptions options)
    {
        using var context = contextFactory.CreateDbContext();
        var settings = context.LoginRateLimitSettings.Find(LoginRateLimitSettings.SingletonId);

        if (settings is null)
        {
            return;
        }

        options.PermitLimit = settings.PermitLimit;
        options.WindowSeconds = settings.WindowSeconds;
    }
}
namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Registrerer og kører seeding af sikkerhedspolicy-indstillinger. Kører i BÅDE
/// Development og Production — modsat AddDatabaseSeeds (abonnementsdata), som kun
/// kører i Development — da rate limiting, JIT-eskalering og step-up re-auth alle
/// afhænger af, at disse rækker findes. Se IdentityBootstrapSeeder for samme mønster.
/// </summary>
public static class SecurityPolicySettingsSeedExtensions
{
    public static IServiceCollection AddSecurityPolicySettingsSeeding(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LoginRateLimitOptions>(configuration.GetSection(LoginRateLimitOptions.SectionName));

        services.AddScoped(sp => CreateSeeder<JitElevationPolicySettings, JitElevationPolicyOptions>(
            sp, o => new JitElevationPolicySettings
            {
                MinRequestedMinutes = o.MinRequestedMinutes,
                MaxRequestedMinutes = o.MaxRequestedMinutes,
                SweepIntervalMinutes = o.SweepIntervalMinutes
            }));

        services.AddScoped(sp => CreateSeeder<ReAuthenticationPolicySettings, ReAuthenticationPolicyOptions>(
            sp, o => new ReAuthenticationPolicySettings { MaxAgeMinutes = o.MaxAgeMinutes }));

        services.AddScoped(sp => CreateSeeder<ReAuthFailureTrackerSettings, ReAuthFailureTrackerOptions>(
            sp, o => new ReAuthFailureTrackerSettings { Threshold = o.Threshold, WindowDays = o.WindowDays }));

        services.AddScoped(sp => CreateSeeder<AdminApiRateLimitSettings, AdminApiRateLimitOptions>(
            sp, o => new AdminApiRateLimitSettings
            {
                PermitLimit = o.PermitLimit,
                WindowSeconds = o.WindowSeconds,
                SegmentsPerWindow = o.SegmentsPerWindow
            }));

        services.AddScoped(sp => CreateSeeder<LoginRateLimitSettings, LoginRateLimitOptions>(
            sp, o => new LoginRateLimitSettings { PermitLimit = o.PermitLimit, WindowSeconds = o.WindowSeconds }));

        return services;
    }

    public static async Task SeedSecurityPolicySettingsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        await sp.GetRequiredService<SecurityPolicySettingsSeeder<JitElevationPolicySettings>>().SeedAsync();
        await sp.GetRequiredService<SecurityPolicySettingsSeeder<ReAuthenticationPolicySettings>>().SeedAsync();
        await sp.GetRequiredService<SecurityPolicySettingsSeeder<ReAuthFailureTrackerSettings>>().SeedAsync();
        await sp.GetRequiredService<SecurityPolicySettingsSeeder<AdminApiRateLimitSettings>>().SeedAsync();
        await sp.GetRequiredService<SecurityPolicySettingsSeeder<LoginRateLimitSettings>>().SeedAsync();
    }

    private static SecurityPolicySettingsSeeder<TEntity> CreateSeeder<TEntity, TOptions>(
        IServiceProvider sp, Func<TOptions, TEntity> map)
        where TEntity : class, ISingletonSettings
        where TOptions : class
    {
        var defaults = sp.GetRequiredService<IOptions<TOptions>>().Value;
        return new SecurityPolicySettingsSeeder<TEntity>(
            sp.GetRequiredService<IAdminDbContextFactory>(), () => map(defaults));
    }
}
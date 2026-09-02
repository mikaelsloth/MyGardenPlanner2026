namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Kobler de 5 sikkerhedspolicy-options-typer til databasen som endelig kilde og gør dem
/// hot-reloadable via IOptionsMonitor&lt;T&gt;. VIGTIGT: skal kaldes EFTER de øvrige
/// AddXxxServices-metoder i Program.cs (AddJitElevationServices, AddReAuthenticationServices,
/// AddReAuthFailureTracking, AddAdminApiRateLimiting, AddSecurityPolicySettingsSeeding), da
/// deres appsettings-baserede services.Configure&lt;T&gt;(...) skal registreres FØR
/// IConfigureOptions&lt;T&gt; herfra, for at DB-værdien korrekt vinder over appsettings.
/// </summary>
public static class SecurityPolicyRuntimeReloadServicesExtensions
{
    public static IServiceCollection AddSecurityPolicyRuntimeReload(this IServiceCollection services)
    {
        services.TryAddSingleton<SecurityPolicyChangeSignal>();
        services.TryAddSingleton<ISecurityPolicyChangeSignal>(sp => sp.GetRequiredService<SecurityPolicyChangeSignal>());

        AddReloadable<JitElevationPolicyOptions, JitElevationPolicyOptionsConfigurator>(services);
        AddReloadable<ReAuthenticationPolicyOptions, ReAuthenticationPolicyOptionsConfigurator>(services);
        AddReloadable<ReAuthFailureTrackerOptions, ReAuthFailureTrackerOptionsConfigurator>(services);
        AddReloadable<AdminApiRateLimitOptions, AdminApiRateLimitOptionsConfigurator>(services);
        AddReloadable<LoginRateLimitOptions, LoginRateLimitOptionsConfigurator>(services);

        services.AddScoped<IJitElevationPolicyAdminService, JitElevationPolicyAdminService>();
        services.AddScoped<IReAuthenticationPolicyAdminService, ReAuthenticationPolicyAdminService>();
        services.AddScoped<IReAuthFailureTrackerPolicyAdminService, ReAuthFailureTrackerPolicyAdminService>();
        services.AddScoped<IAdminApiRateLimitPolicyAdminService, AdminApiRateLimitPolicyAdminService>();
        services.AddScoped<ILoginRateLimitPolicyAdminService, LoginRateLimitPolicyAdminService>();

        return services;
    }

    private static void AddReloadable<TOptions, TConfigurator>(IServiceCollection services)
        where TOptions : class
        where TConfigurator : class, IConfigureOptions<TOptions>
    {
        services.AddSingleton<IConfigureOptions<TOptions>, TConfigurator>();
        services.AddSingleton<IOptionsChangeTokenSource<TOptions>>(sp =>
            new SecurityPolicyOptionsChangeTokenSource<TOptions>(sp.GetRequiredService<SecurityPolicyChangeSignal>()));
    }
}
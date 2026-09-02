namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Overskriver JitElevationPolicyOptions med den aktuelle værdi fra
/// admin.JitElevationPolicySettings, hver gang IOptionsMonitor&lt;JitElevationPolicyOptions&gt;
/// genberegner CurrentValue (dvs. når ISecurityPolicyChangeSignal.TriggerChange udløses).
/// Registreres EFTER services.Configure&lt;JitElevationPolicyOptions&gt;(appsettings-sektion)
/// i DI-rækkefølgen, så DB-værdien vinder når rækken er seedet. Findes rækken (endnu) ikke,
/// bevares appsettings-værdien uændret som fallback.
/// </summary>
public sealed class JitElevationPolicyOptionsConfigurator(IAdminDbContextFactory contextFactory)
    : IConfigureOptions<JitElevationPolicyOptions>
{
    public void Configure(JitElevationPolicyOptions options)
    {
        using var context = contextFactory.CreateDbContext();
        var settings = context.JitElevationPolicySettings.Find(JitElevationPolicySettings.SingletonId);

        if (settings is null)
        {
            return;
        }

        options.MinRequestedMinutes = settings.MinRequestedMinutes;
        options.MaxRequestedMinutes = settings.MaxRequestedMinutes;
        options.SweepIntervalMinutes = settings.SweepIntervalMinutes;
    }
}
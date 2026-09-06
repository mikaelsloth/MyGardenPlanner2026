namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class AuditLogServicesExtensions
{
    public static IServiceCollection AddAuditLogServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<IAuditLogViewerPreferenceService, AuditLogViewerPreferenceService>();
        services.AddScoped<IAuditLogExportService, AuditLogExportService>();

        services.AddDataProtection();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IAuditLogExportTokenService, AuditLogExportTokenService>();

        return services;
    }
}
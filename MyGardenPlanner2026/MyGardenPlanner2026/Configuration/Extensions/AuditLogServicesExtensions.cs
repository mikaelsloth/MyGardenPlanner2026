namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class AuditLogServicesExtensions
{
    public static IServiceCollection AddAuditLogServices(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<IAuditLogViewerPreferenceService, AuditLogViewerPreferenceService>();

        return services;
    }
}
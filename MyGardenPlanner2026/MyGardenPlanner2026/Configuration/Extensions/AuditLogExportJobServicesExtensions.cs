namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using MyGardenPlanner2026.Infrastructure.Services.AuditLog.ExportJobs;

public static class AuditLogExportJobServicesExtensions
{
    public static IServiceCollection AddAuditLogExportJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuditLogExportJobOptions>(
            configuration.GetSection(AuditLogExportJobOptions.SectionName));

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IAuditLogExportJobProcessor, AuditLogExportJobProcessor>();
        services.AddHostedService<AuditLogExportBackgroundService>();

        return services;
    }
}
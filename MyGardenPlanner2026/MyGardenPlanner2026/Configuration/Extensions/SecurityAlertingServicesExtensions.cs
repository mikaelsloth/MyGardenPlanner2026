namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.Extensions.DependencyInjection.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;

public static class SecurityAlertingServicesExtensions
{
    public static IServiceCollection AddSecurityAlertingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));

        // TryAdd: undgår dobbeltregistrering hvis AddReAuthenticationServices/AddJitElevationServices
        // allerede har registreret TimeProvider.System.
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<ISecurityEmailSender, SmtpSecurityEmailSender>();
        services.AddSingleton<ISecurityAlertService, SecurityAlertService>();

        return services;
    }
}
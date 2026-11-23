namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Infrastructure.KeyRing;

public static class DataProtectionServicesExtensions
{
    public static IServiceCollection AddDataProtectionServices(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var options = configuration.GetSection(KeyProtectionOptions.SectionName).Get<KeyProtectionOptions>()
            ?? new KeyProtectionOptions();

        return services.AddPersistedDataProtection(options, requireCertificate: environment.IsProduction());
    }
}
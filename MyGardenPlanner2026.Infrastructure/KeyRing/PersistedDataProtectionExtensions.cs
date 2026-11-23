namespace MyGardenPlanner2026.Infrastructure.KeyRing;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Data;

public static class PersistedDataProtectionExtensions
{
    /// <summary>Fast og må ikke ændres: ændring ugyldiggør cookies, tokens og eksisterende backup-filer.</summary>
    public const string ApplicationName = "MyGardenPlanner";

    /// <summary>
    /// Registrerer Data Protection med nøgler i admin.DataProtectionKeys (via IAdminDbContextFactory, som
    /// skal være registreret) og ApplicationName. Nøglerne krypteres med certifikat, når et er konfigureret.
    /// </summary>
    public static IServiceCollection AddPersistedDataProtection(
        this IServiceCollection services, KeyProtectionOptions options, bool requireCertificate)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var certificate = KeyRingCertificateLoader.Load(options, requireCertificate);

        var builder = services.AddDataProtection().SetApplicationName(ApplicationName);

        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
            new ConfigureOptions<KeyManagementOptions>(keyOptions =>
                keyOptions.XmlRepository = new AdminDbXmlRepository(sp.GetRequiredService<IAdminDbContextFactory>())));

        if (certificate is not null)
        {
            builder.ProtectKeysWithCertificate(certificate);
        }

        return services;
    }
}
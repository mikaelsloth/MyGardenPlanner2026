namespace MyGardenPlanner2026.Infrastructure.KeyRing;

using System.Security.Cryptography.X509Certificates;

/// <summary>Konfiguration af kryptering af Data Protection-nøgler (sektion "DataProtection").</summary>
public sealed class KeyProtectionOptions
{
    public const string SectionName = "DataProtection";

    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }
    public string? CertificateThumbprint { get; set; }
    public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.CurrentUser;
}
namespace MyGardenPlanner2026.Infrastructure.KeyRing;

using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Henter certifikatet, der krypterer nøgleringen. Sti (PFX) har forrang for thumbprint. Uden konfiguration
/// returneres null (nøglerne gemmes ukrypteret), medmindre <c>required</c> er true.
/// </summary>
public static class KeyRingCertificateLoader
{
    public static X509Certificate2? Load(KeyProtectionOptions options, bool required)
    {
        ArgumentNullException.ThrowIfNull(options);

        X509Certificate2? certificate = null;

        if (!string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            certificate = X509CertificateLoader.LoadPkcs12FromFile(options.CertificatePath, options.CertificatePassword);
        }
        else if (!string.IsNullOrWhiteSpace(options.CertificateThumbprint))
        {
            certificate = FindInStore(options);
        }

        if (certificate is null)
        {
            return required
                ? throw new InvalidOperationException(
                    "DataProtection: certifikat mangler. Angiv DataProtection:CertificatePath (+ CertificatePassword) " +
                    "eller DataProtection:CertificateThumbprint. Nøgleringen krypteres kun uden certifikat i Development.")
                : null;
        }

        if (!certificate.HasPrivateKey)
        {
            certificate.Dispose();
            throw new InvalidOperationException(
                "DataProtection: certifikatet har ingen privat nøgle, så nøglerne kan ikke dekrypteres.");
        }

        return certificate;
    }

    private static X509Certificate2 FindInStore(KeyProtectionOptions options)
    {
        using var store = new X509Store(StoreName.My, options.CertificateStoreLocation);
        store.Open(OpenFlags.ReadOnly);

        var matches = store.Certificates.Find(
            X509FindType.FindByThumbprint, options.CertificateThumbprint!.Trim(), validOnly: false);

        return matches.Count == 1
            ? matches[0]
            : throw new InvalidOperationException(
                $"DataProtection: forventede ét certifikat med thumbprint '{options.CertificateThumbprint}' i " +
                $"{options.CertificateStoreLocation}\\My, men fandt {matches.Count}.");
    }
}
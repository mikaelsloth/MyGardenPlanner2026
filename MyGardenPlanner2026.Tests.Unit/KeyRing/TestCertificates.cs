namespace MyGardenPlanner2026.Tests.Unit.KeyRing;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class TestCertificates
{
    /// <summary>Skriver et selvsigneret PFX-certifikat til en midlertidig fil. Kalderen sletter filen.</summary>
    public static string CreatePfxFile(string password)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=MyGardenPlanner-Test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        var path = Path.Combine(Path.GetTempPath(), $"mgp-dp-test-{Guid.NewGuid():N}.pfx");
        File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, password));
        return path;
    }
}
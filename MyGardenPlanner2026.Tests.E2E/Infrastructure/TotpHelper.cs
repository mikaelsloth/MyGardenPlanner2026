namespace MyGardenPlanner2026.Tests.E2E.Infrastructure;

using OtpNet;

/// <summary>
/// Udregner et gyldigt 6-cifret TOTP-kode ud fra en rå Base32 authenticator-nøgle —
/// matcher ASP.NET Core Identitys standardopsætning (SHA1, 30 sek. trin, 6 cifre),
/// så vi undgår at skulle scanne en QR-kode i browseren under tests.
/// </summary>
public static class TotpHelper
{
    public static string GenerateCode(string base32Secret)
    {
        var keyBytes = Base32Encoding.ToBytes(base32Secret);
        return new Totp(keyBytes).ComputeTotp();
    }
}
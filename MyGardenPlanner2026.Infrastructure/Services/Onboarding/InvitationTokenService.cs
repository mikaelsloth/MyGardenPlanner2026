namespace MyGardenPlanner2026.Infrastructure.Services.Onboarding;

using MyGardenPlanner2026.Core.Contracts.Onboarding;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Standardimplementering af IInvitationTokenService. Det rå token er 32 bytes (256 bit)
/// fra en kryptografisk sikker RNG, Base64Url-kodet uden padding. TokenHash er en SHA-256
/// hash (64 lowercase hex-tegn) af det rå token. Med 256 bit entropi er brute-force af et
/// gyldigt token beregningsmæssigt umuligt, og da opslag sker via en indekseret ligheds-
/// forespørgsel på TokenHash (ikke en byte-for-byte app-lags-sammenligning), giver et
/// almindeligt databaseopslag ikke et timing-orakel, der kan udnyttes til at gætte hashen.
/// </summary>
public sealed class InvitationTokenService : IInvitationTokenService
{
    private const int TokenSizeInBytes = 32;

    public InvitationToken GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
        var rawToken = Base64UrlEncode(tokenBytes);

        return new InvitationToken(rawToken, HashToken(rawToken));
    }

    public string HashToken(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexStringLower(hashBytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
namespace MyGardenPlanner2026.Core.Contracts.Account;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Hasher en e-mail til PendingGardenRestore.OwnerEmailHash: trim + lowercase, derefter
/// SHA-256 som 64 lowercase hex-tegn. Samme funktion bruges ved sletning og ved gendannelse.
/// </summary>
public static class RestoreEmailHasher
{
    public static string Hash(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var normalized = email.Trim().ToLowerInvariant();
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }
}
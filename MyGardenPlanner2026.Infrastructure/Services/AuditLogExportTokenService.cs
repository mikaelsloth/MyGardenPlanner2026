namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;
using System.Security.Cryptography;

/// <summary>
/// Implementerer IAuditLogExportTokenService via ASP.NET Core Data Protection —
/// undgår at skulle håndtere egne signeringsnøgler. Payload: "{userId}|{issuedAtUtc:O}".
/// Genbruger ReAuthenticationPolicyOptions.MaxAgeMinutes (samme runtime-konfigurerbare
/// policy som styrer step-up-friskhed i StepUpGuard/RequireRecentAuthenticationHandler),
/// så eksport-tokenets levetid altid matcher admins seneste indstilling. TimeProvider
/// injiceres for testbarhed (samme mønster som ReAuthenticationService/JitElevationService).
/// </summary>
public sealed class AuditLogExportTokenService(
    IDataProtectionProvider dataProtectionProvider,
    IOptionsMonitor<ReAuthenticationPolicyOptions> policyOptionsMonitor,
    TimeProvider timeProvider) : IAuditLogExportTokenService
{
    private const string ProtectorPurpose = "MyGardenPlanner2026.AuditLogExportToken.v1";
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

    public string IssueToken(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var payload = $"{userId}|{timeProvider.GetUtcNow():O}";
        return protector.Protect(payload);
    }

    public bool TryValidateToken(string token, string userId, out string? failureReason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        if (string.IsNullOrWhiteSpace(token))
        {
            failureReason = "Tokenet mangler.";
            return false;
        }

        string payload;
        try
        {
            payload = protector.Unprotect(token);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            failureReason = "Tokenet kunne ikke valideres.";
            return false;
        }

        var parts = payload.Split('|', 2);
        if (parts.Length != 2)
        {
            failureReason = "Tokenets format er ugyldigt.";
            return false;
        }

        if (parts[0] != userId)
        {
            failureReason = "Tokenet tilhører en anden bruger.";
            return false;
        }

        if (!DateTimeOffset.TryParse(parts[1], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var issuedAt))
        {
            failureReason = "Tokenets tidsstempel kunne ikke læses.";
            return false;
        }

        var maxAge = TimeSpan.FromMinutes(policyOptionsMonitor.CurrentValue.MaxAgeMinutes);
        if (timeProvider.GetUtcNow() - issuedAt > maxAge)
        {
            failureReason = "Tokenet er udløbet. Bekræft din identitet igen.";
            return false;
        }

        failureReason = null;
        return true;
    }
}
namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Standardimplementering af IAccountBackupService. Signatur = Data Protection-token (Protect) over et
/// SHA-256-hash af en KANONISK serialisering af filens øvrige felter (format, version, tidspunkt, e-mail-hash
/// og indhold, personlige data sorteret). Verifikation gen-serialiserer det indlæste indhold på samme måde,
/// så omformatering af filen er uden betydning, mens enhver ændring af indholdet gør signaturen ugyldig.
/// Signaturen dækker e-mail-hashen, så den ikke kan udskiftes.
///
/// OBS: Verifikation kræver samme Data Protection-nøglering som ved oprettelsen. Nøgleringen skal
/// persisteres og sikkerhedskopieres, og ApplicationName skal være stabilt, ellers kan gamle filer ikke
/// verificeres (se PR-beskrivelsen).
/// </summary>
public sealed class AccountBackupService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider) : IAccountBackupService
{
    public const string FormatName = "MyGardenPlanner.AccountBackup";
    public const int CurrentVersion = 1;

    private const string ProtectorPurpose = "MyGardenPlanner.AccountBackup.v1";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions FileOptions = new(JsonOptions) { WriteIndented = true };

    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);

    public async Task<byte[]> CreateAsync(
        string userId, string email, IReadOnlyDictionary<string, string> personalData,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentNullException.ThrowIfNull(personalData);

        var payload = new AccountBackupPayload(personalData, await LoadGardensAsync(userId, cancellationToken));
        var createdUtc = timeProvider.GetUtcNow();
        var emailHash = RestoreEmailHasher.Hash(email);

        var hash = ComputeHash(FormatName, CurrentVersion, createdUtc, emailHash, payload);
        var file = new AccountBackupFile(FormatName, CurrentVersion, createdUtc, emailHash, payload, protector.Protect(hash));

        return JsonSerializer.SerializeToUtf8Bytes(file, FileOptions);
    }

    public AccountBackupVerification Verify(byte[] backupFile, string email)
    {
        ArgumentNullException.ThrowIfNull(backupFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        AccountBackupFile? file;
        try
        {
            file = JsonSerializer.Deserialize<AccountBackupFile>(backupFile, JsonOptions);
        }
        catch (JsonException)
        {
            return Rejected(AccountBackupStatus.InvalidFormat);
        }

        if (file is null
            || file.Format != FormatName
            || file.Payload?.PersonalData is null
            || file.Payload.Gardens is null
            || string.IsNullOrWhiteSpace(file.EmailHash)
            || string.IsNullOrWhiteSpace(file.Signature))
        {
            return Rejected(AccountBackupStatus.InvalidFormat);
        }

        if (file.Version != CurrentVersion)
        {
            return Rejected(AccountBackupStatus.UnsupportedVersion);
        }

        var expectedHash = ComputeHash(file.Format, file.Version, file.CreatedUtc, file.EmailHash, file.Payload);
        return !SignatureMatches(file.Signature, expectedHash)
            ? Rejected(AccountBackupStatus.InvalidSignature)
            : !string.Equals(file.EmailHash, RestoreEmailHasher.Hash(email), StringComparison.Ordinal)
            ? Rejected(AccountBackupStatus.EmailMismatch)
            : new AccountBackupVerification(AccountBackupStatus.Valid, file.Payload);
    }

    private static AccountBackupVerification Rejected(AccountBackupStatus status) => new(status, null);

    private bool SignatureMatches(string signature, string expectedHash)
    {
        try
        {
            var actualHash = protector.Unprotect(signature);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(actualHash), Encoding.UTF8.GetBytes(expectedHash));
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            return false;
        }
    }

    private static string ComputeHash(
        string format, int version, DateTimeOffset createdUtc, string emailHash, AccountBackupPayload payload)
    {
        var personalData = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in payload.PersonalData)
        {
            personalData[key] = value;
        }

        var signedPart = new SignedPart(format, version, createdUtc, emailHash, personalData, payload.Gardens);
        var canonicalBytes = JsonSerializer.SerializeToUtf8Bytes(signedPart, JsonOptions);

        return Convert.ToHexStringLower(SHA256.HashData(canonicalBytes));
    }

    private async Task<IReadOnlyList<AccountBackupGarden>> LoadGardensAsync(string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Query-filtrene udelader soft-slettede medlemskaber og haver.
        var memberships = await context.GardenMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
        {
            return [];
        }

        var gardenIds = memberships.Select(m => m.GardenId).ToList();

        var gardens = await context.Gardens
            .AsNoTracking()
            .Where(g => gardenIds.Contains(g.Id))
            .ToListAsync(cancellationToken);

        var entitlements = await context.UserEntitlements
            .AsNoTracking()
            .Where(e => e.UserId == userId && gardenIds.Contains(e.GardenId))
            .ToListAsync(cancellationToken);

        return
        [
            .. memberships
                .Join(gardens, m => m.GardenId, g => g.Id, (m, g) => (Membership: m, Garden: g))
                .OrderBy(x => x.Garden.Id)
                .Select(x =>
                {
                    var entitlement = entitlements.FirstOrDefault(e => e.GardenId == x.Garden.Id);

                    return new AccountBackupGarden(
                        x.Garden.Id, x.Garden.Name, x.Garden.Description, x.Garden.Archived, x.Garden.CreatedAtUtc,
                        x.Membership.IsOwner, x.Membership.Layer, x.Membership.Category, x.Membership.JoinedAtUtc,
                        entitlement is null
                            ? null
                            : new AccountBackupEntitlement(
                                entitlement.Layer, entitlement.Category, entitlement.BillingCycle, entitlement.ValidToUtc,
                                entitlement.IsTrial, entitlement.ExtraBedProposalsCount, entitlement.ExtraPlannedBedsCount,
                                entitlement.ExtraCategoryAArtifactsCount, entitlement.ExtraCategoryBArtifactsCount));
                })
        ];
    }

    /// <summary>Det signerede indhold i kanonisk form (personlige data sorteret). Rækkefølgen af felter er fast.</summary>
    private sealed record SignedPart(
        string Format,
        int Version,
        DateTimeOffset CreatedUtc,
        string EmailHash,
        SortedDictionary<string, string> PersonalData,
        IReadOnlyList<AccountBackupGarden> Gardens);
}
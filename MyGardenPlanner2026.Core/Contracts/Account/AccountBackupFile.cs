namespace MyGardenPlanner2026.Core.Contracts.Account;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Kuverten i en backup-fil (R10.2). Filen er læsbar JSON. <paramref name="Signature"/> er en Data
/// Protection-token over et hash af alle øvrige felter (se IAccountBackupService), så filen ikke kan
/// ændres uden at signaturen fejler. <paramref name="EmailHash"/> (RestoreEmailHasher) binder filen
/// til en e-mail.
/// </summary>
public sealed record AccountBackupFile(
    string Format,
    int Version,
    DateTimeOffset CreatedUtc,
    string EmailHash,
    AccountBackupPayload Payload,
    string Signature);

/// <summary>Brugerens data i en backup: personlige data (som PersonalData.json) og egne haver.</summary>
public sealed record AccountBackupPayload(
    IReadOnlyDictionary<string, string> PersonalData,
    IReadOnlyList<AccountBackupGarden> Gardens);

/// <summary>
/// En af brugerens haver. Indeholder kun brugerens egne oplysninger: ingen andre medlemmers bruger-id'er
/// eller e-mails.
/// </summary>
public sealed record AccountBackupGarden(
    Guid GardenId,
    string Name,
    string? Description,
    bool Archived,
    DateTimeOffset CreatedAtUtc,
    bool IsOwner,
    GardenAccessLevel Layer,
    AccessCategory Category,
    DateTimeOffset JoinedAtUtc,
    AccountBackupEntitlement? Entitlement);

public sealed record AccountBackupEntitlement(
    GardenAccessLevel Layer,
    AccessCategory Category,
    BillingCycle BillingCycle,
    DateTimeOffset? ValidToUtc,
    bool IsTrial,
    int ExtraBedProposalsCount,
    int ExtraPlannedBedsCount,
    int ExtraCategoryAArtifactsCount,
    int ExtraCategoryBArtifactsCount);
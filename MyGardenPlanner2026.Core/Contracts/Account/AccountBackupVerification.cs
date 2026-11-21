namespace MyGardenPlanner2026.Core.Contracts.Account;

public enum AccountBackupStatus
{
    /// <summary>Filen er ægte, uændret og hører til den angivne e-mail.</summary>
    Valid = 0,

    /// <summary>Ikke en backup-fil (ugyldig JSON, forkert format eller mangler felter).</summary>
    InvalidFormat = 1,

    /// <summary>Filens version understøttes ikke.</summary>
    UnsupportedVersion = 2,

    /// <summary>Signaturen er ugyldig: filen er ændret, eller den er ikke lavet af denne installation.</summary>
    InvalidSignature = 3,

    /// <summary>Filen er ægte, men tilhører en anden e-mail.</summary>
    EmailMismatch = 4
}

/// <summary>Resultat af verifikation. <see cref="Payload"/> er kun sat, når status er Valid.</summary>
public sealed record AccountBackupVerification(AccountBackupStatus Status, AccountBackupPayload? Payload)
{
    public bool IsValid => Status == AccountBackupStatus.Valid;
}
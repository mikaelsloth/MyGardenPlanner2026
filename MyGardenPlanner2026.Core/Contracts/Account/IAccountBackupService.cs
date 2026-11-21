namespace MyGardenPlanner2026.Core.Contracts.Account;

/// <summary>
/// Opretter og verificerer signerede backup-filer med brugerens data (R10.2). Der er endnu ingen
/// import: Verify er beregnet til en senere importer, som SKAL angive den nye brugers BEKRÆFTEDE e-mail.
/// </summary>
public interface IAccountBackupService
{
    /// <summary>
    /// Bygger en signeret backup-fil (UTF-8 JSON). <paramref name="personalData"/> er brugerens personlige
    /// data (samme indhold som PersonalData.json); haverne læses fra databasen. Filen bindes til
    /// <paramref name="email"/> via et hash.
    /// </summary>
    Task<byte[]> CreateAsync(
        string userId, string email, IReadOnlyDictionary<string, string> personalData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verificerer en backup-fil mod <paramref name="email"/>: format, version, signatur og e-mail (i den
    /// rækkefølge). Kaster aldrig på ugyldigt indhold, men returnerer en status.
    /// </summary>
    AccountBackupVerification Verify(byte[] backupFile, string email);
}
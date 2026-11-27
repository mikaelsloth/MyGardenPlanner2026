namespace MyGardenPlanner2026.Core.Contracts.Email;

/// <summary>
/// Sender mails til almindelige brugere (bekræftelseslinks, notifikationer, engangskoder).
/// Adskilt fra ISecurityEmailSender, som kun sender sikkerhedsalarmer til administratorer.
/// </summary>
public interface IUserEmailSender
{
    /// <summary>
    /// Sender mailen. Returnerer true, hvis SMTP-serveren modtog den, og false, hvis afsendelsen
    /// ikke var mulig (manglende konfiguration, ugyldig modtager, transportfejl eller timeout).
    /// Kaster ikke ved transportfejl, så en mailfejl aldrig vælter det kaldende flow.
    /// </summary>
    Task<bool> SendAsync(string toAddress, EmailContent content, CancellationToken cancellationToken = default);
}
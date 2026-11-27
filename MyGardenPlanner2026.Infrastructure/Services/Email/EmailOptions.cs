namespace MyGardenPlanner2026.Infrastructure.Services.Email;

/// <summary>
/// Konfiguration for mails til brugere, bundet fra sektionen "Email". Selve SMTP-forbindelsen
/// (Host, Port, credentials) læses fra <c>SmtpOptions</c>.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public EmailProvider Provider { get; set; } = EmailProvider.NoOp;
    public string FromAddress { get; set; } = "noreply@mygardenplanner.dk";
    public string FromDisplayName { get; set; } = "MyGardenPlanner";
}
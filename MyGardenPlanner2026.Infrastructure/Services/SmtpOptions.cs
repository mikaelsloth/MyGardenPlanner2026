namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfiguration for intern SMTP-relay til sikkerhedsalarmer (§4.2), bundet fra
/// appsettings/User Secrets under sektionen "Smtp". Credentials hører ALDRIG i
/// appsettings.json — sæt UserName/Password via 'dotnet user-secrets' lokalt og
/// miljøvariabler/secret store i produktion (samme mønster som InitialAdmin).
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 25;
    public bool EnableSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "sikkerhed@mygardenplanner.dk";
    public string FromDisplayName { get; set; } = "MyGardenPlanner Sikkerhed";
    public List<string> AdminSecurityEmails { get; set; } = [];
}
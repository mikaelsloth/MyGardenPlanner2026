namespace MyGardenPlanner2026.Infrastructure.Services.Email;

/// <summary>Hvilken afsender Identity bruger til mails (konfigureres i Email:Provider).</summary>
public enum EmailProvider
{
    /// <summary>Sender intet. Links kan vises på siden, hvis Identity:ShowEmailLinks er slået til.</summary>
    NoOp = 0,

    /// <summary>Sender via SMTP-opsætningen i sektionen "Smtp".</summary>
    Smtp = 1
}
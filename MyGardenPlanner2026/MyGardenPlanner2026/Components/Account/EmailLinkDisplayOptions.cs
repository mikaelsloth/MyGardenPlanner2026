namespace MyGardenPlanner2026.Components.Account;

/// <summary>
/// Styrer, om bekræftelseslinks vises direkte på siden, når appen bruger
/// <see cref="IdentityNoOpEmailSender"/> (ingen rigtig e-mailudbyder). Links uden ejerskabsbevis
/// må kun vises i udvikling/test. Fjernes, når en rigtig IEmailSender er på plads.
/// </summary>
internal sealed class EmailLinkDisplayOptions
{
    public const string SectionName = "Identity";

    /// <summary>Standard false: links vises aldrig, medmindre det slås til eksplicit.</summary>
    public bool ShowEmailLinks { get; set; }
}
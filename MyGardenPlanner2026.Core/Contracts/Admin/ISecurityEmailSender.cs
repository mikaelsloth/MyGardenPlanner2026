namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Lavniveau e-mail-afsender til sikkerhedsalarmer. Adskilt fra Identity's
/// IEmailSender&lt;ApplicationUser&gt; (bekræftelses-/nulstillingsmails), da modtagerne
/// her er den faste admin-sikkerhedsliste, ikke en enkelt bruger.
/// </summary>
public interface ISecurityEmailSender
{
    Task SendAsync(IReadOnlyList<string> toAddresses, string subject, string body, CancellationToken cancellationToken = default);
}
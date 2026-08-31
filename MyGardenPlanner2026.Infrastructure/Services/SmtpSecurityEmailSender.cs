namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Net;
using System.Net.Mail;

/// <summary>
/// SMTP-baseret ISecurityEmailSender via System.Net.Mail (indbygget i .NET, ingen
/// NuGet-afhængighed) — passer til on-prem/no-cost kravet (§4.2). Fejler "blødt": hvis
/// Host eller AdminSecurityEmails mangler, logges en advarsel og der returneres uden at
/// kaste, så en manglende SMTP-konfiguration aldrig kan vælte det kaldende flow (fx JIT-
/// godkendelse eller rate limit-afvisning).
/// </summary>
public sealed class SmtpSecurityEmailSender(
    IOptions<SmtpOptions> options, ILogger<SmtpSecurityEmailSender> logger) : ISecurityEmailSender
{
    public async Task SendAsync(
        IReadOnlyList<string> toAddresses, string subject, string body, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        if (toAddresses is null || toAddresses.Count == 0)
        {
            logger.LogWarning(
                "Sikkerhedsalarm '{Subject}' blev IKKE sendt: ingen modtagere er konfigureret i Smtp:AdminSecurityEmails.",
                subject);
            return;
        }

        var smtpOptions = options.Value;
        if (string.IsNullOrWhiteSpace(smtpOptions.Host))
        {
            logger.LogWarning(
                "Sikkerhedsalarm '{Subject}' blev IKKE sendt: SMTP-server (Smtp:Host) er ikke konfigureret.",
                subject);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(smtpOptions.FromAddress, smtpOptions.FromDisplayName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        foreach (var address in toAddresses)
        {
            message.To.Add(address);
        }

        using var client = new SmtpClient(smtpOptions.Host, smtpOptions.Port)
        {
            EnableSsl = smtpOptions.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(smtpOptions.UserName))
        {
            client.Credentials = new NetworkCredential(smtpOptions.UserName, smtpOptions.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
namespace MyGardenPlanner2026.Infrastructure.Services.Email;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Email;
using MyGardenPlanner2026.Infrastructure.Services.SecurityAlerting;
using System.Net;
using System.Net.Mail;
using System.Text;

/// <summary>
/// Midlertidig SMTP-implementering af IUserEmailSender via System.Net.Mail. Forbindelsen
/// (Host, Port, EnableSsl, credentials, timeout) læses fra <see cref="SmtpOptions"/>, afsenderen
/// fra <see cref="EmailOptions"/>. Fejler "blødt": mangler Host, er modtageren ugyldig, eller fejler
/// transporten, logges en advarsel og der returneres false uden at kaste.
/// Modtagerens adresse logges aldrig (persondata).
/// </summary>
public sealed partial class SmtpUserEmailSender(
    IOptionsMonitor<SmtpOptions> smtpOptions,
    IOptionsMonitor<EmailOptions> emailOptions,
    ILogger<SmtpUserEmailSender> logger) : IUserEmailSender
{
    [LoggerMessage(EventId = 1171, Level = LogLevel.Warning, Message = "Mail '{Subject}' blev IKKE sendt: SMTP-server (Smtp:Host) er ikke konfigureret.")]
    static partial void SmtpHostNotConfigured(ILogger logger, string Subject);

    [LoggerMessage(EventId = 1172, Level = LogLevel.Warning, Message = "Mail '{Subject}' blev IKKE sendt: modtageradressen er ugyldig.")]
    static partial void InvalidRecipient(ILogger logger, string Subject);

    [LoggerMessage(EventId = 1173, Level = LogLevel.Warning, Message = "Mail '{Subject}' blev IKKE sendt: SMTP-afsendelsen fejlede.")]
    static partial void SendFailed(ILogger logger, string Subject, Exception exception);

    [LoggerMessage(EventId = 1174, Level = LogLevel.Warning, Message = "Mail '{Subject}' blev IKKE sendt: SMTP-afsendelsen blev afbrudt efter {TimeoutSeconds} sekunder.")]
    static partial void SendTimedOut(ILogger logger, string Subject, int TimeoutSeconds);

    public async Task<bool> SendAsync(
        string toAddress, EmailContent content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toAddress);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(content.Subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(content.HtmlBody);

        var smtp = smtpOptions.CurrentValue;
        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            SmtpHostNotConfigured(logger, content.Subject);
            return false;
        }

        if (!MailAddress.TryCreate(toAddress, out var recipient))
        {
            InvalidRecipient(logger, content.Subject);
            return false;
        }

        var sender = emailOptions.CurrentValue;
        using var message = new MailMessage
        {
            From = new MailAddress(sender.FromAddress, sender.FromDisplayName, Encoding.UTF8),
            Subject = content.Subject,
            SubjectEncoding = Encoding.UTF8,
            Body = content.HtmlBody,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        message.To.Add(recipient);

        var timeoutSeconds = smtp.TimeoutSeconds > 0 ? smtp.TimeoutSeconds : SmtpOptions.DefaultTimeoutSeconds;

#pragma warning disable S5332
        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Timeout = (int)Math.Min(timeoutSeconds * 1000L, int.MaxValue)
        };
#pragma warning restore S5332

        if (!string.IsNullOrWhiteSpace(smtp.UserName))
        {
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            await client.SendMailAsync(message, timeoutSource.Token);
            return true;
        }
        catch (SmtpException ex)
        {
            SendFailed(logger, content.Subject, ex);
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            SendTimedOut(logger, content.Subject, timeoutSeconds);
            return false;
        }
    }
}
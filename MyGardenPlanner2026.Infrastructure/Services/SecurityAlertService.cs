namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Standardimplementering af ISecurityAlertService. Logger strukturerede events via
/// kildegenererede LoggerMessage-metoder (Windows Event Log/Serilog-klar uden
/// kodeændringer) og videresender til ISecurityEmailSender. TimeProvider injiceres for
/// testbarhed (samme mønster som JitElevationService/ReAuthenticationService).
/// </summary>
public sealed partial class SecurityAlertService(
    ISecurityEmailSender emailSender,
    IOptions<SmtpOptions> smtpOptions,
    TimeProvider timeProvider,
    ILogger<SecurityAlertService> logger) : ISecurityAlertService
{
    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning,
        Message = "SIKKERHEDSALARM: Gentagne fejlede re-auth/MFA-forsøg registreret for bruger '{UserId}' fra IP {IpAddress}.")]
    static partial void FailedReAuthDetected(ILogger logger, string UserId, string IpAddress);

    [LoggerMessage(EventId = 2002, Level = LogLevel.Warning,
        Message = "SIKKERHEDSALARM: JIT-admin-rollen '{RoleName}' er godkendt/aktiveret for bruger '{RequesterId}'.")]
    static partial void JitElevationGranted(ILogger logger, string RequesterId, string RoleName);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning,
        Message = "SIKKERHEDSALARM: Kritisk sikkerhedspolicy '{PolicyName}' blev ændret af bruger '{UserId}'.")]
    static partial void SecurityPolicyChanged(ILogger logger, string UserId, string PolicyName);

    public async Task AlertFailedReAuthAsync(string userId, string ip, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);

        FailedReAuthDetected(logger, userId, ip);

        await SendAsync(
            "[MyGardenPlanner] Sikkerhedsalarm: Gentagne fejlede login-/re-auth-forsøg",
            BuildBody(
                $"Bruger '{userId}' har haft gentagne fejlede MFA/re-auth-forsøg.",
                [("Bruger-ID", userId), ("IP-adresse", ip)]),
            cancellationToken);
    }

    public async Task AlertJitRequestedAsync(string requesterId, string role, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        JitElevationGranted(logger, requesterId, role);

        await SendAsync(
            "[MyGardenPlanner] Sikkerhedsalarm: JIT-admin-rettighed tildelt/aktiveret",
            BuildBody(
                $"Der er tildelt/aktiveret midlertidig admin-rolle '{role}' til bruger '{requesterId}'.",
                [("Bruger-ID", requesterId), ("Rolle", role)]),
            cancellationToken);
    }

    public async Task AlertPolicyChangedAsync(string userId, string policyName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        SecurityPolicyChanged(logger, userId, policyName);

        await SendAsync(
            "[MyGardenPlanner] Sikkerhedsalarm: Sikkerhedspolicy ændret",
            BuildBody(
                $"Bruger '{userId}' har ændret den kritiske sikkerhedspolicy '{policyName}'.",
                [("Bruger-ID", userId), ("Policy", policyName)]),
            cancellationToken);
    }

    private Task SendAsync(string subject, string body, CancellationToken cancellationToken) =>
        emailSender.SendAsync(smtpOptions.Value.AdminSecurityEmails, subject, body, cancellationToken);

    private string BuildBody(string summary, IReadOnlyList<(string Label, string Value)> fields)
    {
        var timestamp = timeProvider.GetUtcNow().ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

        var lines = new List<string> { summary, string.Empty, $"Tidspunkt: {timestamp}" };
        lines.AddRange(fields.Select(f => $"{f.Label}: {f.Value}"));

        return string.Join(Environment.NewLine, lines);
    }
}
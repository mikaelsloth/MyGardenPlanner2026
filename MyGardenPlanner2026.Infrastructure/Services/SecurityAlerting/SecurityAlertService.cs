namespace MyGardenPlanner2026.Infrastructure.Services.SecurityAlerting;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Globalization;

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
        Message = "SIKKERHEDSALARM: JIT-admin-rollen '{RoleName}' er godkendt/aktiveret for bruger '{RequesterId}' af bruger '{ApproverId}' i {Minutes} minutter.")]
    static partial void JitElevationGranted(ILogger logger, string RequesterId, string ApproverId, string RoleName, int Minutes);

    [LoggerMessage(EventId = 2004, Level = LogLevel.Information,
        Message = "SIKKERHEDSALARM: Bruger '{RequesterId}' har anmodet om JIT-admin-rollen '{RoleName}' i {Minutes} minutter.")]
    static partial void JitElevationRequested(ILogger logger, string RequesterId, string RoleName, int Minutes);

    [LoggerMessage(EventId = 2003, Level = LogLevel.Warning,
        Message = "SIKKERHEDSALARM: Kritisk sikkerhedspolicy '{PolicyName}' blev ændret af bruger '{UserId}'.")]
    static partial void SecurityPolicyChanged(ILogger logger, string UserId, string PolicyName);

    public Task AlertFailedReAuthAsync(string userId, string ip, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(ip);

        FailedReAuthDetected(logger, userId, ip);

        return SendAsync(
            "[MyGardenPlanner] Sikkerhedsalarm: Gentagne fejlede login-/re-auth-forsøg",
            BuildBody(
                $"Bruger '{userId}' har haft gentagne fejlede MFA/re-auth-forsøg.",
                [("Bruger-ID", userId), ("IP-adresse", ip)]),
            cancellationToken);
    }

    public Task AlertJitRequestedAsync(string requesterId, string role, int minutes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minutes);

        JitElevationRequested(logger, requesterId, role, minutes);

        var minutesText = minutes.ToString(CultureInfo.InvariantCulture);

        return SendAsync(
            "[MyGardenPlanner] Sikkerhedsalarm: JIT-adgang anmodet",
            BuildBody(
                $"Bruger '{requesterId}' har anmodet om midlertidig admin-rolle '{role}' i {minutesText} minutter.",
                [("Bruger-ID", requesterId), ("Rolle", role), ("Minutter", minutesText)]),
            cancellationToken);
    }

    public Task AlertJitApprovedAsync(
        string requesterId, string approverId, string role, int minutes, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requesterId);
        ArgumentException.ThrowIfNullOrWhiteSpace(approverId);
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minutes);

        JitElevationGranted(logger, requesterId, approverId, role, minutes);

        var minutesText = minutes.ToString(CultureInfo.InvariantCulture);

        return SendAsync(
            "[MyGardenPlanner] Sikkerhedsalarm: JIT-admin-rettighed tildelt/aktiveret",
            BuildBody(
                $"Der er tildelt/aktiveret midlertidig admin-rolle '{role}' til bruger '{requesterId}' i {minutesText} minutter, godkendt af bruger '{approverId}'.",
                [("Bruger-ID", requesterId), ("Godkender-ID", approverId), ("Rolle", role), ("Minutter", minutesText)]),
            cancellationToken);
    }

    public Task AlertPolicyChangedAsync(string userId, string policyName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        SecurityPolicyChanged(logger, userId, policyName);

        return SendAsync(
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
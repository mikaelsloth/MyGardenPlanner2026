namespace MyGardenPlanner2026.Tests.E2E;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Smtp;

/// <summary>
/// Sikkerhedsalarmen "Sikkerhedspolicy ændret" (SecurityAlertService.AlertPolicyChangedAsync). Sendes ved
/// hver vellykket Gem og indeholder linjerne "Bruger-ID: &lt;id&gt;" og "Policy: &lt;navn&gt;".
/// </summary>
public static class SecurityAlerts
{
    public const string PolicyChangedSubject = "[MyGardenPlanner] Sikkerhedsalarm: Sikkerhedspolicy ændret";

    public static bool IsPolicyChangedMail(ReceivedMail mail, string userId) =>
        mail.Subject == PolicyChangedSubject
        && mail.Body.Contains($"Bruger-ID: {userId}", StringComparison.Ordinal);

    /// <summary>Venter på den første ændringsmail fra brugeren og kontrollerer modtager og policy-navn.</summary>
    public static async Task ExpectPolicyChangedAsync(TestSmtpServer smtp, string userId, string policyName)
    {
        var mail = await smtp.WaitForMessageAsync(m => IsPolicyChangedMail(m, userId));

        mail.Recipients.Should().Equal(PlaywrightAppFixture.SecurityAlertRecipient);
        mail.Body.Should().Contain($"Policy: {policyName}");
    }

    /// <summary>Antal "Sikkerhedspolicy ændret"-mails fra brugeren for den navngivne policy.</summary>
    public static int CountPolicyChangedMails(IEnumerable<ReceivedMail> mails, string userId, string policyName) =>
        mails.Count(m => IsPolicyChangedMail(m, userId)
            && m.Body.Contains($"Policy: {policyName}", StringComparison.Ordinal));
}
namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Smtp;

public sealed class SecurityAlertsTests
{
    private static ReceivedMail Mail(string subject, string body) =>
        new("sikkerhed@mygardenplanner.dk", ["modtager@test.dk"], subject, body);

    private const string Body =
        "Bruger 'u1' har ændret den kritiske sikkerhedspolicy 'JitElevationPolicy'.\r\n\r\n" +
        "Tidspunkt: 2026-10-18 12:00:00 UTC\r\nBruger-ID: u1\r\nPolicy: JitElevationPolicy";

    [Fact]
    public void IsPolicyChangedMail_RigtigtEmneOgBruger_ErSand()
    {
        SecurityAlerts.IsPolicyChangedMail(Mail(SecurityAlerts.PolicyChangedSubject, Body), "u1").Should().BeTrue();
    }

    [Fact]
    public void IsPolicyChangedMail_AndenBruger_ErFalsk()
    {
        SecurityAlerts.IsPolicyChangedMail(Mail(SecurityAlerts.PolicyChangedSubject, Body), "u2").Should().BeFalse();
    }

    [Fact]
    public void IsPolicyChangedMail_AndetEmne_ErFalsk()
    {
        SecurityAlerts.IsPolicyChangedMail(Mail(PlaywrightAppFixture.SecurityAlertSubject, Body), "u1").Should().BeFalse();
    }

    [Fact]
    public void CountPolicyChangedMails_TaellerKunMatchendeBrugerOgPolicy()
    {
        var mails = new[]
        {
            Mail(SecurityAlerts.PolicyChangedSubject, Body),
            Mail(SecurityAlerts.PolicyChangedSubject, Body.Replace("JitElevationPolicy", "ReAuthenticationPolicy")),
            Mail(SecurityAlerts.PolicyChangedSubject, Body.Replace("Bruger-ID: u1", "Bruger-ID: u2")),
            Mail(PlaywrightAppFixture.SecurityAlertSubject, Body),
        };

        SecurityAlerts.CountPolicyChangedMails(mails, "u1", "JitElevationPolicy").Should().Be(1);
        SecurityAlerts.CountPolicyChangedMails(mails, "u1", "ReAuthenticationPolicy").Should().Be(1);
        SecurityAlerts.CountPolicyChangedMails(mails, "u3", "JitElevationPolicy").Should().Be(0);
    }
}
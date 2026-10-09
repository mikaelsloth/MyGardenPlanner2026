namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Tests.E2E.Smtp;
using System.Net.Mail;

/// <summary>Sender via samme System.Net.Mail.SmtpClient som appen, så protokol og kodning verificeres reelt.</summary>
public sealed class TestSmtpServerTests
{
    private const string AlertSubject = "[MyGardenPlanner] Sikkerhedsalarm: Gentagne fejlede login-/re-auth-forsøg";

    [Fact]
    public async Task Modtager_MailMedDanskeTegn_DekoderetKorrekt()
    {
        await using var server = TestSmtpServer.Start();
        var body = string.Join(
            Environment.NewLine,
            "Bruger 'abc' har haft gentagne fejlede MFA/re-auth-forsøg.",
            string.Empty,
            "Tidspunkt: 2026-10-09 12:00:00 UTC",
            "Bruger-ID: 1234-5678",
            "IP-adresse: 127.0.0.1");

        await SendAsync(server.Port, AlertSubject, body, "modtager@test.dk");

        var mail = await server.WaitForMessageAsync(_ => true, TimeSpan.FromSeconds(5));

        mail.From.Should().Be("sikkerhed@mygardenplanner.dk");
        mail.Recipients.Should().Equal("modtager@test.dk");
        mail.Subject.Should().Be(AlertSubject);
        mail.Body.Should().Contain("forsøg").And.Contain("Bruger-ID: 1234-5678").And.Contain("IP-adresse: 127.0.0.1");
    }

    [Fact]
    public async Task Modtager_LangLinjeMedDanskeTegn_BevaresUbrudt()
    {
        await using var server = TestSmtpServer.Start();
        var line = $"Bruger-ID: {new string('x', 120)} forsøg til sidst";

        await SendAsync(server.Port, "Emne", line, "modtager@test.dk");

        var mail = await server.WaitForMessageAsync(_ => true, TimeSpan.FromSeconds(5));

        mail.Body.Should().Contain(line);
    }

    [Fact]
    public async Task WaitForMessageAsync_UdenMail_KasterTimeout()
    {
        await using var server = TestSmtpServer.Start();

        var act = () => server.WaitForMessageAsync(_ => true, TimeSpan.FromMilliseconds(200));

        await act.Should().ThrowAsync<TimeoutException>();
    }

    private static async Task SendAsync(int port, string subject, string body, string to)
    {
        using var message = new MailMessage
        {
            From = new MailAddress("sikkerhed@mygardenplanner.dk", "MyGardenPlanner Sikkerhed"),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        using var client = new SmtpClient("127.0.0.1", port) { EnableSsl = false };
        await client.SendMailAsync(message, TestContext.Current.CancellationToken);
    }
}
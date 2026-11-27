namespace MyGardenPlanner2026.Tests.E2E.Smtp;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Services.Email;
using MyGardenPlanner2026.Infrastructure.Services.SecurityAlerting;

public sealed class SmtpUserEmailSenderTests
{
    private const string EncodedLink = "https://localhost/x?a=1&amp;b=2";

    [Fact]
    public async Task SendAsync_DeliversDanishHtmlMailToTheSink()
    {
        await using var sink = TestSmtpServer.Start();
        var sut = CreateSender(sink.Port);

        var sent = await sut.SendAsync(
            "modtager@e2e.test", EmailTemplates.ConfirmEmail(EncodedLink), TestContext.Current.CancellationToken);

        sent.Should().BeTrue();
        var mail = await sink.WaitForMessageAsync(m => m.Recipients.Contains("modtager@e2e.test"));
        mail.Subject.Should().Be("Bekræft din e-mail – MyGardenPlanner");
        mail.Body.Should().Contain(EncodedLink);
        mail.Body.Should().Contain("Bekræft e-mail");
    }

    private static SmtpUserEmailSender CreateSender(int port) =>
        new(
            new FixedMonitor<SmtpOptions>(new SmtpOptions { Host = "127.0.0.1", Port = port, EnableSsl = false }),
            new FixedMonitor<EmailOptions>(new EmailOptions()),
            NullLogger<SmtpUserEmailSender>.Instance);

    private sealed class FixedMonitor<T>(T value) : IOptionsMonitor<T>
        where T : class
    {
        public T CurrentValue => value;

        public T Get(string? name) => value;

        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
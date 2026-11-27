namespace MyGardenPlanner2026.Tests.Unit.Services.Email;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Email;
using MyGardenPlanner2026.Infrastructure.Services.Email;
using MyGardenPlanner2026.Infrastructure.Services.SecurityAlerting;
using NSubstitute;
using System.Net;
using System.Net.Sockets;

public sealed class SmtpUserEmailSenderTests
{
    private static readonly EmailContent Content = new("Emne", "<p>Indhold</p>");

    private static SmtpUserEmailSender Create(SmtpOptions smtp)
    {
        var smtpMonitor = Substitute.For<IOptionsMonitor<SmtpOptions>>();
        smtpMonitor.CurrentValue.Returns(smtp);
        var emailMonitor = Substitute.For<IOptionsMonitor<EmailOptions>>();
        emailMonitor.CurrentValue.Returns(new EmailOptions());
        return new SmtpUserEmailSender(smtpMonitor, emailMonitor, NullLogger<SmtpUserEmailSender>.Instance);
    }

    [Fact]
    public async Task SendAsync_WithoutHost_ReturnsFalse()
    {
        var sut = Create(new SmtpOptions { Host = string.Empty });

        var sent = await sut.SendAsync("a@b.dk", Content, TestContext.Current.CancellationToken);

        sent.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WithInvalidRecipient_ReturnsFalseWithoutConnecting()
    {
        var sut = Create(new SmtpOptions { Host = "127.0.0.1", Port = 1 });

        var sent = await sut.SendAsync("ikke-en-mail", Content, TestContext.Current.CancellationToken);

        sent.Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenServerIsUnreachable_ReturnsFalseWithoutThrowing()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var closedPort = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var sut = Create(new SmtpOptions
        {
            Host = "127.0.0.1",
            Port = closedPort,
            EnableSsl = false,
            TimeoutSeconds = 2
        });

        var sent = await sut.SendAsync("a@b.dk", Content, TestContext.Current.CancellationToken);

        sent.Should().BeFalse();
    }

    [Theory]
    [InlineData("", "<p>x</p>")]
    [InlineData("Emne", " ")]
    public async Task SendAsync_RejectsBlankSubjectOrBody(string subject, string body)
    {
        var sut = Create(new SmtpOptions { Host = "127.0.0.1" });

        var act = async () => await sut.SendAsync(
            "a@b.dk", new EmailContent(subject, body), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
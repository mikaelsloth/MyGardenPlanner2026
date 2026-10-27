namespace MyGardenPlanner2026.Tests.Unit.Services.SecurityAlerting;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Services.SecurityAlerting;
using NSubstitute;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using Xunit;

/// <summary>
/// Tester de "bløde fejl"-grene. Manglende modtagere/Host returnerer FØR SmtpClient oprettes.
/// Transportfejl testes mod en lukket loopback-port (ingen netværksadgang eller SMTP-server nødvendig).
/// En vellykket afsendelse kræver en SMTP-fanger og dækkes af E2E (TestSmtpServer).
/// </summary>
public class SmtpSecurityEmailSenderTests
{
    private static SmtpSecurityEmailSender CreateSender(SmtpOptions? options = null) =>
        new(Options.Create(options ?? new SmtpOptions()), Substitute.For<ILogger<SmtpSecurityEmailSender>>());

    [Fact]
    public async Task SendAsync_EmptySubject_ThrowsArgumentException()
    {
        var sender = CreateSender();

        var act = async () => await sender.SendAsync(["a@b.dk"], "", "body");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendAsync_EmptyBody_ThrowsArgumentException()
    {
        var sender = CreateSender();

        var act = async () => await sender.SendAsync(["a@b.dk"], "subject", "");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendAsync_NoRecipients_ReturnsWithoutThrowing()
    {
        var sender = CreateSender();

        var act = async () => await sender.SendAsync([], "subject", "body");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_NullRecipients_ReturnsWithoutThrowing()
    {
        var sender = CreateSender();

        var act = async () => await sender.SendAsync(null!, "subject", "body");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_NoHostConfigured_ReturnsWithoutThrowing()
    {
        var sender = CreateSender(new SmtpOptions { Host = "" });

        var act = async () => await sender.SendAsync(["a@b.dk"], "subject", "body");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_SmtpTransportFails_LogsWarningAndDoesNotThrow()
    {
        var logger = Substitute.For<ILogger<SmtpSecurityEmailSender>>();
        logger.IsEnabled(LogLevel.Warning).Returns(true);
        var sender = new SmtpSecurityEmailSender(
            Options.Create(new SmtpOptions { Host = "127.0.0.1", Port = GetUnusedLoopbackPort(), EnableSsl = false }),
            logger);

        var act = async () => await sender.SendAsync(
            ["a@b.dk"], "subject", "body", TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Is<EventId>(e => e.Id == 1133),
            Arg.Any<Arg.AnyType>(),
            Arg.Any<SmtpException>(),
            Arg.Any<Func<Arg.AnyType, Exception?, string>>());
    }

    [Fact]
    public async Task SendAsync_ServerNeverResponds_AbortsAfterTimeoutAndDoesNotThrow()
    {
        // Lytter uden Accept: forbindelsen oprettes, men serveren sender aldrig 220-hilsenen.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            var logger = Substitute.For<ILogger<SmtpSecurityEmailSender>>();
            logger.IsEnabled(LogLevel.Warning).Returns(true);
            var sender = new SmtpSecurityEmailSender(
                Options.Create(new SmtpOptions { Host = "127.0.0.1", Port = port, EnableSsl = false, TimeoutSeconds = 1 }),
                logger);

            var stopwatch = Stopwatch.StartNew();
            var act = async () => await sender.SendAsync(
                ["a@b.dk"], "subject", "body", TestContext.Current.CancellationToken);

            await act.Should().NotThrowAsync();

            stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
            // Afbrydelsen kan give enten SmtpException (1133) eller timeout (1134), afhængigt af SmtpClient.
            logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Is<EventId>(e => e.Id == 1133 || e.Id == 1134),
                Arg.Any<Arg.AnyType>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<Arg.AnyType, Exception?, string>>());
        }
        finally
        {
            listener.Stop();
        }
    }

    private static int GetUnusedLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}
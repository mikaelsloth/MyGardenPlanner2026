namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

/// <summary>
/// Tester udelukkende de "bløde fejl"-grene (manglende modtagere/Host), der returnerer
/// FØR SmtpClient oprettes — kan derfor køre uden netværksadgang eller reel SMTP-server.
/// Selve afsendelsen (SmtpClient.SendMailAsync) kræver en integrationstest mod en lokal
/// SMTP-fanger (fx smtp4dev/Papercut) og dækkes ikke her.
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
}
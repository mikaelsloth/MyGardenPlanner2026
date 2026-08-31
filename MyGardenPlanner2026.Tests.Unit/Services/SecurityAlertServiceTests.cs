namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class SecurityAlertServiceTests
{
    private static readonly string[] AdminEmails = ["sikkerhed@mygardenplanner.dk"];

    private static (SecurityAlertService Service, ISecurityEmailSender EmailSender) CreateService()
    {
        var emailSender = Substitute.For<ISecurityEmailSender>();
        var smtpOptions = Options.Create(new SmtpOptions { AdminSecurityEmails = [.. AdminEmails] });
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero));
        var logger = Substitute.For<ILogger<SecurityAlertService>>();

        return (new SecurityAlertService(emailSender, smtpOptions, timeProvider, logger), emailSender);
    }

    [Fact]
    public async Task AlertFailedReAuthAsync_SendsEmail_ToConfiguredAdminRecipients()
    {
        var (service, emailSender) = CreateService();

        await service.AlertFailedReAuthAsync("user-1", "10.0.0.5", TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Is<IReadOnlyList<string>>(r => r.SequenceEqual(AdminEmails)),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlertFailedReAuthAsync_EmailBody_ContainsUserIdAndIpAddress()
    {
        var (service, emailSender) = CreateService();

        await service.AlertFailedReAuthAsync("user-1", "10.0.0.5", TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("user-1") && body.Contains("10.0.0.5")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlertJitRequestedAsync_EmailBody_ContainsRequesterIdAndRole()
    {
        var (service, emailSender) = CreateService();

        await service.AlertJitRequestedAsync("user-2", "SystemAdmin", TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("user-2") && body.Contains("SystemAdmin")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlertPolicyChangedAsync_EmailBody_ContainsUserIdAndPolicyName()
    {
        var (service, emailSender) = CreateService();

        await service.AlertPolicyChangedAsync("user-3", "RequireRecentAuthentication", TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("user-3") && body.Contains("RequireRecentAuthentication")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AlertFailedReAuthAsync_EmailBody_ContainsTimestampFromTimeProvider()
    {
        var (service, emailSender) = CreateService();

        await service.AlertFailedReAuthAsync("user-1", "10.0.0.5", TestContext.Current.CancellationToken);

        await emailSender.Received(1).SendAsync(
            Arg.Any<IReadOnlyList<string>>(), Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("2026-08-31 10:00:00 UTC")),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null, "10.0.0.5")]
    [InlineData("user-1", null)]
    [InlineData("", "10.0.0.5")]
    public async Task AlertFailedReAuthAsync_MissingArguments_ThrowsArgumentException(string? userId, string? ip)
    {
        var (service, _) = CreateService();

        var act = async () => await service.AlertFailedReAuthAsync(userId!, ip!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
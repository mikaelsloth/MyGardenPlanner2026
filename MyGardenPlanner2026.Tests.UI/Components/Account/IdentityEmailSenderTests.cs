namespace MyGardenPlanner2026.Tests.UI.Components.Account;

using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Core.Contracts.Email;
using MyGardenPlanner2026.Core.Entities;
using NSubstitute;

public sealed class IdentityEmailSenderTests
{
    private const string Link = "https://localhost/x?a=1&amp;b=2";

    private readonly IUserEmailSender inner = Substitute.For<IUserEmailSender>();
    private readonly ApplicationUser user = new() { Email = "a@b.dk" };

    [Fact]
    public async Task SendConfirmationLinkAsync_SendsConfirmationMailToTheAddress()
    {
        var sut = new IdentityEmailSender(inner);

        await sut.SendConfirmationLinkAsync(user, "a@b.dk", Link);

        await inner.Received(1).SendAsync(
            Arg.Is<string>(a => a == "a@b.dk"),
            Arg.Is<EmailContent>(c => c.Subject.StartsWith("Bekræft") && c.HtmlBody.Contains(Link)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendPasswordResetLinkAsync_SendsResetMailToTheAddress()
    {
        var sut = new IdentityEmailSender(inner);

        await sut.SendPasswordResetLinkAsync(user, "a@b.dk", Link);

        await inner.Received(1).SendAsync(
            Arg.Is<string>(a => a == "a@b.dk"),
            Arg.Is<EmailContent>(c => c.Subject.StartsWith("Nulstil") && c.HtmlBody.Contains(Link)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendPasswordResetCodeAsync_SendsTheCode()
    {
        var sut = new IdentityEmailSender(inner);

        await sut.SendPasswordResetCodeAsync(user, "a@b.dk", "123456");

        await inner.Received(1).SendAsync(
            Arg.Is<string>(a => a == "a@b.dk"),
            Arg.Is<EmailContent>(c => c.HtmlBody.Contains("123456")),
            Arg.Any<CancellationToken>());
    }
}
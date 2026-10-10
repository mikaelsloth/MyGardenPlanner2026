namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages.Manage;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Components.Account.Pages.Manage;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public class EmailTests : BunitContext
{
    private const string CurrentEmail = "gammel@example.dk";
    private const string NewEmail = "ny@example.dk";

    // Base64Url("token") uden padding.
    private const string EncodedToken = "dG9rZW4";

    private const string ChangeEmailForm = "form:not(#send-verification-form)";
    private const string ConfirmationLink = "a[href*='ConfirmEmailChange']";
    private const string SentMessage = "Confirmation link to change email sent. Please check your email.";

    [Fact]
    public async Task OnValidSubmitAsync_NoOpEmailSender_ShowsConfirmationLinkWithCallbackUrl()
    {
        var (userManager, _) = CreateUserManagerWithUser();
        Services.AddSingleton(userManager);
        Services.AddSingleton<IEmailSender<ApplicationUser>>(new IdentityNoOpEmailSender());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();
        await cut.Find("#Input\\.NewEmail").ChangeAsync(NewEmail);
        await cut.Find(ChangeEmailForm).SubmitAsync();

        cut.Markup.Should().Contain(SentMessage);

        var link = cut.Find(ConfirmationLink);
        link.TextContent.Should().Contain("Click here to confirm your new email");

        var href = link.GetAttribute("href");
        href.Should().Contain("Account/ConfirmEmailChange");
        href.Should().Contain("userId=user-1");
        href.Should().Contain($"email={Uri.EscapeDataString(NewEmail)}");
        href.Should().Contain($"code={EncodedToken}");
    }

    [Fact]
    public async Task OnValidSubmitAsync_OtherEmailSender_SendsLinkWithoutShowingItOnPage()
    {
        var (userManager, _) = CreateUserManagerWithUser();
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();
        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();
        await cut.Find("#Input\\.NewEmail").ChangeAsync(NewEmail);
        await cut.Find(ChangeEmailForm).SubmitAsync();

        cut.Markup.Should().Contain(SentMessage);
        cut.FindAll(ConfirmationLink).Should().BeEmpty();
        await emailSender.Received().SendConfirmationLinkAsync(
            Arg.Is<ApplicationUser>(u => u.Id == "user-1"),
            Arg.Is<string>(e => e == NewEmail),
            Arg.Any<string>());
    }

    [Fact]
    public async Task OnValidSubmitAsync_UnchangedEmail_ShowsUnchangedMessageWithoutLinkOrToken()
    {
        var (userManager, _) = CreateUserManagerWithUser();
        Services.AddSingleton(userManager);
        Services.AddSingleton<IEmailSender<ApplicationUser>>(new IdentityNoOpEmailSender());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();

        // Feltet "New email" er forudfyldt med den nuværende e-mail.
        await cut.Find(ChangeEmailForm).SubmitAsync();

        cut.Markup.Should().Contain("Your email is unchanged.");
        cut.FindAll(ConfirmationLink).Should().BeEmpty();
        await userManager.DidNotReceive().GenerateChangeEmailTokenAsync(
            Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    private static (UserManager<ApplicationUser> UserManager, ApplicationUser User) CreateUserManagerWithUser()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetEmailAsync(user).Returns(Task.FromResult<string?>(CurrentEmail));
        userManager.IsEmailConfirmedAsync(user).Returns(Task.FromResult(true));
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));
        userManager.GenerateChangeEmailTokenAsync(user, NewEmail).Returns(Task.FromResult("token"));

        return (userManager, user);
    }
}
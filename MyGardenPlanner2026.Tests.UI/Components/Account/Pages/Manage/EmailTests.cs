namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages.Manage;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    private const string VerificationLink = "a[href*='Account/ConfirmEmail?']";
    private const string VerificationSentMessage = "Verification email sent. Please check your email.";
    private const string SendVerificationForm = "#send-verification-form";

    private void RegisterEmailLinkOptions(bool showEmailLinks) =>
        Services.AddSingleton(Options.Create(new EmailLinkDisplayOptions { ShowEmailLinks = showEmailLinks }));

    [Fact]
    public async Task OnValidSubmitAsync_NoOpEmailSender_ShowsConfirmationLinkWithCallbackUrl()
    {
        var (userManager, _) = CreateUserManagerWithUser();
        RegisterEmailLinkOptions(showEmailLinks: true);
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
        RegisterEmailLinkOptions(showEmailLinks: true);
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
        RegisterEmailLinkOptions(showEmailLinks: true);
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

    [Fact]
    public async Task OnValidSubmitAsync_NoOpEmailSenderFlagOff_DoesNotShowConfirmationLink()
    {
        var (userManager, _) = CreateUserManagerWithUser();
        RegisterEmailLinkOptions(showEmailLinks: false);
        Services.AddSingleton(userManager);
        Services.AddSingleton<IEmailSender<ApplicationUser>>(new IdentityNoOpEmailSender());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();
        await cut.Find("#Input\\.NewEmail").ChangeAsync(NewEmail);
        await cut.Find(ChangeEmailForm).SubmitAsync();

        cut.Markup.Should().Contain(SentMessage);
        cut.FindAll(ConfirmationLink).Should().BeEmpty();
    }

    [Fact]
    public async Task OnSendEmailVerificationAsync_NoOpEmailSenderFlagOn_ShowsVerificationLink()
    {
        var (userManager, _) = CreateUserManagerWithUser(emailConfirmed: false);
        RegisterEmailLinkOptions(showEmailLinks: true);
        Services.AddSingleton(userManager);
        Services.AddSingleton<IEmailSender<ApplicationUser>>(new IdentityNoOpEmailSender());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();
        await cut.Find(SendVerificationForm).SubmitAsync();

        cut.Markup.Should().Contain(VerificationSentMessage);
        var link = cut.Find(VerificationLink);
        link.TextContent.Should().Contain("Click here to verify your email");

        var href = link.GetAttribute("href");
        href.Should().Contain("userId=user-1");
        href.Should().Contain($"code={EncodedToken}");
        href.Should().NotContain("ConfirmEmailChange");
    }

    [Fact]
    public async Task OnSendEmailVerificationAsync_NoOpEmailSenderFlagOff_DoesNotShowVerificationLink()
    {
        var (userManager, _) = CreateUserManagerWithUser(emailConfirmed: false);
        RegisterEmailLinkOptions(showEmailLinks: false);
        Services.AddSingleton(userManager);
        Services.AddSingleton<IEmailSender<ApplicationUser>>(new IdentityNoOpEmailSender());
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();
        await cut.Find(SendVerificationForm).SubmitAsync();

        cut.Markup.Should().Contain(VerificationSentMessage);
        cut.FindAll(VerificationLink).Should().BeEmpty();
    }

    [Fact]
    public async Task OnSendEmailVerificationAsync_OtherEmailSenderFlagOn_SendsLinkWithoutShowingIt()
    {
        var (userManager, _) = CreateUserManagerWithUser(emailConfirmed: false);
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();
        RegisterEmailLinkOptions(showEmailLinks: true);
        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        this.UseIdentityRedirectManager();

        var cut = this.RenderWithHttpContext<Email>();
        await cut.Find(SendVerificationForm).SubmitAsync();

        cut.FindAll(VerificationLink).Should().BeEmpty();
        await emailSender.Received().SendConfirmationLinkAsync(
            Arg.Is<ApplicationUser>(u => u.Id == "user-1"),
            Arg.Is<string>(e => e == CurrentEmail),
            Arg.Any<string>());
    }

    private static (UserManager<ApplicationUser> UserManager, ApplicationUser User) CreateUserManagerWithUser(
        bool emailConfirmed = true)
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();

        userManager.GetUserAsync(Arg.Any<ClaimsPrincipal>()).Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetEmailAsync(user).Returns(Task.FromResult<string?>(CurrentEmail));
        userManager.IsEmailConfirmedAsync(user).Returns(Task.FromResult(emailConfirmed));
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));
        userManager.GenerateChangeEmailTokenAsync(user, NewEmail).Returns(Task.FromResult("token"));
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns(Task.FromResult("token"));

        return (userManager, user);
    }
}
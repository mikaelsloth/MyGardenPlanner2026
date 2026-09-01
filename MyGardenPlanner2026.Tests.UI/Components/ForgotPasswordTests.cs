namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class ForgotPasswordTests : BunitContext
{
    [Fact]
    public async Task OnValidSubmitAsync_UnknownEmail_StillRedirectsToConfirmation_WithoutSendingEmail()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("ukendt@example.dk").Returns(Task.FromResult<ApplicationUser?>(null));
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        var navMan = this.UseIdentityRedirectManager();

        var cut = Render<ForgotPassword>();
        await cut.Find("#Input\\.Email").ChangeAsync("ukendt@example.dk");
        await cut.Find("form").SubmitAsync();

        navMan.Uri.Should().Contain("ForgotPasswordConfirmation");
        await emailSender.DidNotReceive().SendPasswordResetLinkAsync(
            Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnValidSubmitAsync_ConfirmedEmail_SendsResetLinkAndRedirects()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("kendt@example.dk").Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.IsEmailConfirmedAsync(user).Returns(Task.FromResult(true));
        userManager.GeneratePasswordResetTokenAsync(user).Returns(Task.FromResult("token"));
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        var navMan = this.UseIdentityRedirectManager();

        var cut = Render<ForgotPassword>();
        await cut.Find("#Input\\.Email").ChangeAsync("kendt@example.dk");
        await cut.Find("form").SubmitAsync();

        navMan.Uri.Should().Contain("ForgotPasswordConfirmation");
        await emailSender.Received().SendPasswordResetLinkAsync(user, "kendt@example.dk", Arg.Any<string>());
    }
}
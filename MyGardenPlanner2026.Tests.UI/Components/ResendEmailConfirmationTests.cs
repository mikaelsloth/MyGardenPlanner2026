namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class ResendEmailConfirmationTests : BunitContext
{
    [Fact]
    public async Task OnValidSubmitAsync_UnknownEmail_ShowsSameMessage_WithoutSendingEmail()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("ukendt@example.dk").Returns(Task.FromResult<ApplicationUser?>(null));
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        this.UseIdentityRedirectManager();

        var cut = Render<ResendEmailConfirmation>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        await cut.Find("#Input\\.Email").ChangeAsync("ukendt@example.dk");
        await cut.Find("form").SubmitAsync();

        cut.Markup.Should().Contain("Bekræftelses-e-mail sendt. Tjek venligst din e-mail.");
        await emailSender.DidNotReceive().SendConfirmationLinkAsync(
            Arg.Any<ApplicationUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task OnValidSubmitAsync_KnownEmail_SendsConfirmationLink()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("kendt@example.dk").Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns(Task.FromResult("token"));
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        this.UseIdentityRedirectManager();

        var cut = Render<ResendEmailConfirmation>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        await cut.Find("#Input\\.Email").ChangeAsync("kendt@example.dk");
        await cut.Find("form").SubmitAsync();

        await emailSender.Received().SendConfirmationLinkAsync(user, "kendt@example.dk", Arg.Any<string>());
    }
}
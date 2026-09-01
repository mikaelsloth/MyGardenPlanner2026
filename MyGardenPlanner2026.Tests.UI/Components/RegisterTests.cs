namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class RegisterTests : BunitContext
{
    [Fact]
    public async Task RegisterUser_ValidInput_CreatesUserSendsEmailAndRedirectsToConfirmation()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.Options = new IdentityOptions();
        userManager.Options.SignIn.RequireConfirmedAccount = true;
        userManager.SupportsUserEmail.Returns(true);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), "P@ssw0rd123!")
            .Returns(Task.FromResult(IdentityResult.Success));
        userManager.GetUserIdAsync(Arg.Any<ApplicationUser>()).Returns(Task.FromResult("user-1"));
        userManager.GenerateEmailConfirmationTokenAsync(Arg.Any<ApplicationUser>())
            .Returns(Task.FromResult("token"));

        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        var userStore = Substitute.For<IUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>>();
        var emailSender = Substitute.For<IEmailSender<ApplicationUser>>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        Services.AddSingleton(userStore);
        Services.AddSingleton(emailSender);
        Services.AddSingleton(Substitute.For<ILogger<Register>>());
        var navMan = this.UseIdentityRedirectManager();

        var cut = Render<Register>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        await cut.Find("#Input\\.Email").ChangeAsync("ny-bruger@example.dk");
        await cut.Find("#Input\\.Password").ChangeAsync("P@ssw0rd123!");
        await cut.Find("#Input\\.ConfirmPassword").ChangeAsync("P@ssw0rd123!");
        await cut.Find("form").SubmitAsync();

        navMan.Uri.Should().Contain("RegisterConfirmation");
        await emailSender.Received().SendConfirmationLinkAsync(
            Arg.Any<ApplicationUser>(), "ny-bruger@example.dk", Arg.Any<string>());
    }

    [Fact]
    public void RegisterUser_CreateAsyncFails_ShowsErrorStatusMessage()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.SupportsUserEmail.Returns(true);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Adgangskoden er for svag." })));

        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        var userStore = Substitute.For<IUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>>();

        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        Services.AddSingleton(userStore);
        Services.AddSingleton(Substitute.For<IEmailSender<ApplicationUser>>());
        Services.AddSingleton(Substitute.For<ILogger<Register>>());
        this.UseIdentityRedirectManager();

        var cut = Render<Register>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));
        cut.Find("#Input\\.Email").Change("ny-bruger@example.dk");
        cut.Find("#Input\\.Password").Change("svagkode123");
        cut.Find("#Input\\.ConfirmPassword").Change("svagkode123");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Adgangskoden er for svag.");
    }
}
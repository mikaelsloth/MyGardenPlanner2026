namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI.Identity;
using NSubstitute;
using Xunit;

public class RegisterConfirmationTests : BunitContext
{
    [Fact]
    public void RegisterConfirmation_UserFound_ShowsCheckEmailMessage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("ny@example.dk").Returns(Task.FromResult<ApplicationUser?>(user));

        Services.AddSingleton(userManager);
        Services.AddSingleton(Substitute.For<IEmailSender<ApplicationUser>>());
        this.UseIdentityRedirectManager();

        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo("/Account/RegisterConfirmation?Email=ny@example.dk");

        var cut = Render<RegisterConfirmation>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Markup.Should().Contain("Tjek venligst din e-mail for at bekræfte din konto.");
    }

    [Fact]
    public void RegisterConfirmation_UserNotFound_ShowsErrorAndSets404()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("ukendt@example.dk").Returns(Task.FromResult<ApplicationUser?>(null));

        Services.AddSingleton(userManager);
        Services.AddSingleton(Substitute.For<IEmailSender<ApplicationUser>>());
        this.UseIdentityRedirectManager();

        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo("/Account/RegisterConfirmation?Email=ukendt@example.dk");

        var httpContext = new DefaultHttpContext();
        var cut = Render<RegisterConfirmation>(parameters => parameters.AddCascadingValue(httpContext));

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        cut.Markup.Should().Contain("Error: Kunne ikke finde bruger for den angivne e-mail.");
    }
}
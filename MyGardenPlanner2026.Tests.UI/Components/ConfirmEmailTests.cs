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

public class ConfirmEmailTests : BunitContext
{
    private const string EncodedCode = "dGVzdC1jb2Rl"; // Base64Url af "test-code"

    [Fact]
    public void ConfirmEmail_UserFoundAndConfirmed_ShowsSuccessMessage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.ConfirmEmailAsync(user, "test-code").Returns(Task.FromResult(IdentityResult.Success));
        Services.AddSingleton(userManager);
        this.UseIdentityRedirectManager();

        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo($"/Account/ConfirmEmail?UserId=user-1&Code={EncodedCode}");

        var httpContext = new DefaultHttpContext();
        var cut = Render<ConfirmEmail>(parameters => parameters.AddCascadingValue(httpContext));

        cut.Markup.Should().Contain("Tak, fordi du bekræftede din e-mail.");
    }

    [Fact]
    public void ConfirmEmail_UserNotFound_ShowsErrorAndSets404()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByIdAsync("missing-user").Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);
        this.UseIdentityRedirectManager();

        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo($"/Account/ConfirmEmail?UserId=missing-user&Code={EncodedCode}");

        var httpContext = new DefaultHttpContext();
        var cut = Render<ConfirmEmail>(parameters => parameters.AddCascadingValue(httpContext));

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        cut.Markup.Should().Contain("Error: Kunne ikke finde bruger med ID missing-user.");
    }
}
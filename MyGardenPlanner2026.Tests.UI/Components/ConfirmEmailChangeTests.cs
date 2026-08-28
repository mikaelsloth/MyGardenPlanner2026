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

public class ConfirmEmailChangeTests : BunitContext
{
    private const string EncodedCode = "dGVzdC1jb2Rl";

    [Fact]
    public void ConfirmEmailChange_AllStepsSucceed_ShowsSuccessMessage()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.ChangeEmailAsync(user, "ny@example.dk", "test-code").Returns(Task.FromResult(IdentityResult.Success));
        userManager.SetUserNameAsync(user, "ny@example.dk").Returns(Task.FromResult(IdentityResult.Success));

        var signInManager = IdentityTestDoubles.CreateSignInManager(userManager);
        Services.AddSingleton(userManager);
        Services.AddSingleton(signInManager);
        this.UseIdentityRedirectManager();

        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo($"/Account/ConfirmEmailChange?UserId=user-1&Email=ny@example.dk&Code={EncodedCode}");

        var cut = Render<ConfirmEmailChange>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        cut.Markup.Should().Contain("Tak, fordi du bekræftede skiftet af din e-mail.");
    }

    [Fact]
    public void ConfirmEmailChange_MissingQueryParameters_RedirectsToLoginWithError()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        Services.AddSingleton(userManager);
        Services.AddSingleton(IdentityTestDoubles.CreateSignInManager(userManager));
        var navMan = this.UseIdentityRedirectManager();

        var cut = Render<ConfirmEmailChange>(parameters => parameters.AddCascadingValue(new DefaultHttpContext()));

        navMan.Uri.Should().Contain("Account/Login");
    }
}
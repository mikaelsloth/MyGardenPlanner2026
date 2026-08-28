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

public class ResetPasswordTests : BunitContext
{
    private const string EncodedCode = "cmVzZXQtdG9rZW4"; // Base64Url af "reset-token"

    [Fact]
    public void OnInitialized_NoCodeInQuery_RedirectsToInvalidPasswordReset()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();

        Render<ResetPassword>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;

        navMan.Uri.Should().Contain("Account/InvalidPasswordReset");
    }

    [Fact]
    public void OnValidSubmitAsync_ResetFails_ShowsErrorFromIdentityResult()
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync("test@example.dk").Returns(Task.FromResult<ApplicationUser?>(user));
        userManager.ResetPasswordAsync(user, "reset-token", "NytKodeord123!")
            .Returns(Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Token er udløbet." })));

        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();
        navMan.NavigateTo($"/Account/ResetPassword?code={EncodedCode}");

        var cut = Render<ResetPassword>(parameters => parameters.AddCascadingValue(new DefaultHttpContext())); ;
        cut.Find("#Input\\.Email").Change("test@example.dk");
        cut.Find("#Input\\.Password").Change("NytKodeord123!");
        cut.Find("#Input\\.ConfirmPassword").Change("NytKodeord123!");
        cut.Find("form").Submit();

        cut.Markup.Should().Contain("Token er udløbet.");
    }
}
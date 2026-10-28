namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class ConfirmEmailTests : BunitContext
{
    private const string EncodedCode = "dGVzdC1jb2Rl"; // Base64Url af "test-code"

    private (UserManager<ApplicationUser> UserManager, ApplicationUser User) Arrange(string code)
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns(Task.FromResult<ApplicationUser?>(user));

        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();
        navMan.NavigateTo($"/Account/ConfirmEmail?UserId=user-1&Code={Uri.EscapeDataString(code)}");

        return (userManager, user);
    }

    [Fact]
    public void ConfirmEmail_ValidCode_ShowsSuccessAndLoginLink()
    {
        var (userManager, user) = Arrange(EncodedCode);
        userManager.ConfirmEmailAsync(user, "test-code").Returns(Task.FromResult(IdentityResult.Success));

        var cut = this.RenderWithHttpContext<ConfirmEmail>();

        cut.Markup.Should().Contain("Tak, fordi du bekræftede din e-mail.");
        cut.Find("a[href='Account/Login']").TextContent.Should().Contain("Klik her for at logge ind");
    }

    [Fact]
    public void ConfirmEmail_WrongToken_ShowsErrorWithoutLoginLink()
    {
        var (userManager, user) = Arrange(EncodedCode);
        userManager.ConfirmEmailAsync(user, "test-code").Returns(Task.FromResult(IdentityResult.Failed()));

        var cut = this.RenderWithHttpContext<ConfirmEmail>();

        cut.Markup.Should().Contain("Error: Der opstod en fejl ved bekræftelse af din e-mail.");
        cut.FindAll("a[href='Account/Login']").Should().BeEmpty();
    }

    [Fact]
    public async Task ConfirmEmail_InvalidCodeFormat_ShowsErrorAndDoesNotConfirm()
    {
        var (userManager, _) = Arrange("!!!");

        var cut = this.RenderWithHttpContext<ConfirmEmail>();

        cut.Markup.Should().Contain("Error: Der opstod en fejl ved bekræftelse af din e-mail.");
        cut.FindAll("a[href='Account/Login']").Should().BeEmpty();
        await userManager.DidNotReceive().ConfirmEmailAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>());
    }

    [Fact]
    public void ConfirmEmail_UnknownUser_Returns404AndShowsError()
    {
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByIdAsync("user-1").Returns(Task.FromResult<ApplicationUser?>(null));
        Services.AddSingleton(userManager);
        var navMan = this.UseIdentityRedirectManager();
        navMan.NavigateTo($"/Account/ConfirmEmail?UserId=user-1&Code={EncodedCode}");
        var httpContext = new DefaultHttpContext();

        var cut = this.RenderWithHttpContext<ConfirmEmail>(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        cut.Markup.Should().Contain("Error: Kunne ikke finde bruger med ID user-1.");
    }

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
        var cut = this.RenderWithHttpContext<ConfirmEmail>(httpContext);

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
        var cut = this.RenderWithHttpContext<ConfirmEmail>(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        cut.Markup.Should().Contain("Error: Kunne ikke finde bruger med ID missing-user.");
    }
}
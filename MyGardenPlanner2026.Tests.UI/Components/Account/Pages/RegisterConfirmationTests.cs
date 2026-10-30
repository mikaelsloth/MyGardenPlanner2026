namespace MyGardenPlanner2026.Tests.UI.Components.Account.Pages;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.UI;
using NSubstitute;
using Xunit;

public class RegisterConfirmationTests : BunitContext
{
    private const string UserEmail = "bruger@example.dk";
    private const string NeutralText = "Tjek venligst din e-mail for at bekræfte din konto.";
    private const string ConfirmLink = "a[href*='Account/ConfirmEmail']";

    // Base64Url("token") uden padding.
    private const string EncodedToken = "dG9rZW4";

    private (UserManager<ApplicationUser> UserManager, BunitNavigationManager NavMan) Arrange(
        IEmailSender<ApplicationUser> emailSender,
        bool showEmailLinks,
        bool userExists = true,
        string extraQuery = "")
    {
        var user = new ApplicationUser { Id = "user-1" };
        var userManager = IdentityTestDoubles.CreateUserManager();
        userManager.FindByEmailAsync(UserEmail).Returns(Task.FromResult<ApplicationUser?>(userExists ? user : null));
        userManager.GetUserIdAsync(user).Returns(Task.FromResult("user-1"));
        userManager.GenerateEmailConfirmationTokenAsync(user).Returns(Task.FromResult("token"));

        Services.AddSingleton(userManager);
        Services.AddSingleton(emailSender);
        Services.AddSingleton(Options.Create(new EmailLinkDisplayOptions { ShowEmailLinks = showEmailLinks }));

        var navMan = this.UseIdentityRedirectManager();
        navMan.NavigateTo($"/Account/RegisterConfirmation?Email={UserEmail}{extraQuery}");

        return (userManager, navMan);
    }

    [Fact]
    public void NoEmailParameter_RedirectsToFrontPage()
    {
        Services.AddSingleton(IdentityTestDoubles.CreateUserManager());
        Services.AddSingleton<IEmailSender<ApplicationUser>>(new IdentityNoOpEmailSender());
        Services.AddSingleton(Options.Create(new EmailLinkDisplayOptions { ShowEmailLinks = true }));
        var navMan = this.UseIdentityRedirectManager();

        this.RenderWithHttpContext<RegisterConfirmation>();

        navMan.Uri.Should().Be(navMan.BaseUri);
    }

    [Fact]
    public void NoOpEmailSenderFlagOn_KnownUser_ShowsConfirmationLinkWithReturnUrl()
    {
        Arrange(new IdentityNoOpEmailSender(), showEmailLinks: true, extraQuery: "&ReturnUrl=%2Fpricing");

        var cut = this.RenderWithHttpContext<RegisterConfirmation>();

        var href = cut.Find(ConfirmLink).GetAttribute("href");
        href.Should().Contain("userId=user-1");
        href.Should().Contain($"code={EncodedToken}");
        href.Should().Contain("returnUrl=%2Fpricing");
        cut.Find(ConfirmLink).TextContent.Should().Contain("Klik her for at bekræfte din konto");
    }

    [Fact]
    public void NoOpEmailSenderFlagOn_UnknownUser_Returns404AndShowsError()
    {
        Arrange(new IdentityNoOpEmailSender(), showEmailLinks: true, userExists: false);
        var httpContext = new DefaultHttpContext();

        var cut = this.RenderWithHttpContext<RegisterConfirmation>(httpContext);

        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        cut.Markup.Should().Contain("Error: Kunne ikke finde bruger for den angivne e-mail.");
        cut.FindAll(ConfirmLink).Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NoOpEmailSenderFlagOff_ShowsNeutralMessageWithoutLookup(bool userExists)
    {
        var (userManager, _) = Arrange(new IdentityNoOpEmailSender(), showEmailLinks: false, userExists: userExists);
        var httpContext = new DefaultHttpContext();

        var cut = this.RenderWithHttpContext<RegisterConfirmation>(httpContext);

        cut.Find("p[role='alert']").TextContent.Should().Contain(NeutralText);
        cut.FindAll(ConfirmLink).Should().BeEmpty();
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        await userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task OtherEmailSenderFlagOn_ShowsNeutralMessageWithoutLookup()
    {
        var (userManager, _) = Arrange(Substitute.For<IEmailSender<ApplicationUser>>(), showEmailLinks: true);

        var cut = this.RenderWithHttpContext<RegisterConfirmation>();

        cut.Find("p[role='alert']").TextContent.Should().Contain(NeutralText);
        cut.FindAll(ConfirmLink).Should().BeEmpty();
        await userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

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

        var cut = this.RenderWithHttpContext<RegisterConfirmation>();

        cut.Markup.Should().Contain("Tjek venligst din e-mail for at bekræfte din konto.");
    }
}
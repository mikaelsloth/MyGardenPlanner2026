namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Components.Account.Pages;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;

public class StaticAccountPagesTests : BunitContext
{
    [Fact]
    public void Lockout_RendersAuthPageShellWithDanishTitleAndDangerStatusMessage()
    {
        var cut = Render<Lockout>();

        cut.Find("h1").TextContent.Should().Be("Konto låst");
        cut.Find(".status-danger").Should().NotBeNull();
    }

    [Fact]
    public void InvalidPasswordReset_RendersAuthPageShellWithDanishTitle()
    {
        var cut = Render<InvalidPasswordReset>();

        cut.Find("h1").TextContent.Should().Be("Ugyldigt link til nulstilling");
        cut.Markup.Should().Contain("er ugyldigt");
    }

    [Fact]
    public void InvalidUser_RendersAuthPageShellAndStatusMessageComponent()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<InvalidUser>(parameters => parameters
            .AddCascadingValue(httpContext));

        cut.Find("h1").TextContent.Should().Be("Ugyldig bruger");
        cut.FindComponent<StatusMessage>().Should().NotBeNull();
    }

    [Fact]
    public void ForgotPasswordConfirmation_RendersAuthPageShellWithDanishMessage()
    {
        var cut = Render<ForgotPasswordConfirmation>();

        cut.Find("h1").TextContent.Should().Be("Tjek din e-mail");
        cut.Markup.Should().Contain("nulstille din adgangskode");
    }

    [Fact]
    public void ResetPasswordConfirmation_RendersAuthPageShellWithLoginLink()
    {
        var cut = Render<ResetPasswordConfirmation>();

        cut.Find("h1").TextContent.Should().Be("Adgangskode nulstillet");
        cut.Find("a[href='Account/Login']").TextContent.Should().Contain("logge ind");
    }
}
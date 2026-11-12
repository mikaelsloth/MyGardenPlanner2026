namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Components.Account;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;
using AccountStatusMessage = MyGardenPlanner2026.Components.Account.Shared.StatusMessage;

public class StatusMessageTests : BunitContext
{
    private IRenderedComponent<AccountStatusMessage> RenderMessage(string? message, HttpContext? httpContext = null)
    {
        var context = httpContext ?? new DefaultHttpContext();

        return Render<AccountStatusMessage>(p => p
            .Add(x => x.Message, message)
            .AddCascadingValue<HttpContext>(context));
    }

    [Fact]
    public void ErrorPrefixedDanishMessage_RendersAsDangerAlert()
    {
        var cut = RenderMessage("Error: Ugyldigt loginforsøg.");

        cut.Find(".status-message.status-danger.status-page").GetAttribute("role").Should().Be("alert");
        cut.FindAll(".status-success").Should().BeEmpty();
        cut.Find(".status-message-text p").TextContent.Should().Be("Error: Ugyldigt loginforsøg.");
    }

    [Fact]
    public void MessageWithoutErrorPrefix_RendersAsSuccessStatus()
    {
        var cut = RenderMessage("Din authenticator-app er bekræftet.");

        var element = cut.Find(".status-message.status-success.status-page");
        element.GetAttribute("role").Should().Be("status");
        element.GetAttribute("aria-live").Should().Be("polite");
        cut.FindAll(".status-danger").Should().BeEmpty();
    }

    [Theory]
    [InlineData("Fejl: Noget gik galt.")]
    [InlineData("error: noget gik galt.")]
    public void OnlyExactEnglishErrorPrefixIsDanger(string message)
    {
        var cut = RenderMessage(message);

        cut.FindAll(".status-danger").Should().BeEmpty();
        cut.FindAll(".status-success").Should().HaveCount(1);
    }

    [Fact]
    public void NoMessage_RendersNothing()
    {
        var cut = RenderMessage(null);

        cut.FindAll(".status-message").Should().BeEmpty();
    }

    [Fact]
    public void MessageFromCookie_IsShown_WithErrorPrefixAsDanger()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{IdentityRedirectManager.StatusCookieName}=Error%3A%20Brugeren%20mangler";

        var cut = RenderMessage(null, context);

        cut.Find(".status-danger[role=alert]").TextContent.Should().Contain("Error: Brugeren mangler");
    }

    [Fact]
    public void MessageParameter_TakesPrecedenceOverCookie()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{IdentityRedirectManager.StatusCookieName}=Error%3A%20Fra%20cookie";

        var cut = RenderMessage("Fra parameter", context);

        cut.Find(".status-message-text p").TextContent.Should().Be("Fra parameter");
        cut.FindAll(".status-danger").Should().BeEmpty();
    }

    [Fact]
    public void StatusMessage_MessageStartingWithError_RendersDangerVariantWithAlertRole()
    {
        var cut = this.RenderWithHttpContext<StatusMessage>(parameterBuilder: p => p
            .Add(x => x.Message, "Error: Ugyldigt login."));

        var message = cut.Find(".status-message");
        message.ClassList.Should().Contain("status-danger");
        message.GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().Contain("Error: Ugyldigt login.");
    }

    [Fact]
    public void StatusMessage_MessageNotStartingWithError_RendersSuccessVariantWithStatusRole()
    {
        var cut = this.RenderWithHttpContext<StatusMessage>(parameterBuilder: p => p
            .Add(x => x.Message, "Din adgangskode er ændret."));

        var message = cut.Find(".status-message");
        message.ClassList.Should().Contain("status-success");
        message.GetAttribute("role").Should().Be("status");
    }

    [Fact]
    public void StatusMessage_NoMessageAndNoCookie_RendersNothing()
    {
        var cut = this.RenderWithHttpContext<StatusMessage>();

        cut.FindAll(".status-message").Should().BeEmpty();
    }
}
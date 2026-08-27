namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;

public class StatusMessageTests : BunitContext
{
    [Fact]
    public void StatusMessage_MessageStartingWithError_RendersDangerVariantWithAlertRole()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<StatusMessage>(parameters => parameters
            .AddCascadingValue(httpContext)
            .Add(p => p.Message, "Error: Ugyldigt login."));

        var message = cut.Find(".status-message");
        message.ClassList.Should().Contain("status-danger");
        message.GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().Contain("Error: Ugyldigt login.");
    }

    [Fact]
    public void StatusMessage_MessageNotStartingWithError_RendersSuccessVariantWithStatusRole()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<StatusMessage>(parameters => parameters
            .AddCascadingValue(httpContext)
            .Add(p => p.Message, "Din adgangskode er ændret."));

        var message = cut.Find(".status-message");
        message.ClassList.Should().Contain("status-success");
        message.GetAttribute("role").Should().Be("status");
    }

    [Fact]
    public void StatusMessage_NoMessageAndNoCookie_RendersNothing()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<StatusMessage>(parameters => parameters.AddCascadingValue(httpContext));

        cut.FindAll(".status-message").Should().BeEmpty();
    }
}
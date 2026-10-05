namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;

public class StatusMessageTests : BunitContext
{
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
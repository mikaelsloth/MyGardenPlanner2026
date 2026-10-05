namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Public;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public class ErrorPageTests : BunitContext
{
    [Fact]
    public void Error_WithTraceIdentifier_ShowsRequestId()
    {
        var cut = this.RenderWithHttpContext<Error>(new DefaultHttpContext { TraceIdentifier = "trace-123" });

        cut.Markup.Should().Contain("trace-123");
        cut.Markup.Should().Contain("Forespørgsels-ID");
    }

    [Fact]
    public void Error_RendersDanishHeadingAndDangerEmptyStateVariant()
    {
        var cut = this.RenderWithHttpContext<Error>();

        cut.Find("h1").TextContent.Should().Be("Der opstod en fejl");
        cut.Find(".empty-state").ClassList.Should().Contain("empty-error");
    }

    [Fact]
    public void Error_RendersDevelopmentModeWarningNote()
    {
        var cut = this.RenderWithHttpContext<Error>();

        cut.Find(".status-warning").Should().NotBeNull();
        cut.Markup.Should().Contain("Udviklingstilstand");
    }
}
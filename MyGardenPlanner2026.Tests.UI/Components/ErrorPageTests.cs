namespace MyGardenPlanner2026.Tests.UI.Components;

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
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-123" };
        var cut = Render<Error>(parameters => parameters
            .AddCascadingValue(httpContext));

        cut.Markup.Should().Contain("trace-123");
        cut.Markup.Should().Contain("Forespørgsels-ID");
    }

    [Fact]
    public void Error_RendersDanishHeadingAndDangerEmptyStateVariant()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<Error>(parameters => parameters
            .AddCascadingValue(httpContext));

        cut.Find("h1").TextContent.Should().Be("Der opstod en fejl");
        cut.Find(".empty-state").ClassList.Should().Contain("empty-error");
    }

    [Fact]
    public void Error_RendersDevelopmentModeWarningNote()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<Error>(parameters => parameters
            .AddCascadingValue(httpContext));

        cut.Find(".status-warning").Should().NotBeNull();
        cut.Markup.Should().Contain("Udviklingstilstand");
    }
}
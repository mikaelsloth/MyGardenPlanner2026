namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public sealed class BunitHttpContextExtensionsTests : BunitContext
{
    [Fact]
    public void RenderWithHttpContext_NoArguments_CascadesDefaultHttpContext()
    {
        var cut = this.RenderWithHttpContext<StatusMessage>();

        cut.FindAll(".status-message").Should().BeEmpty();
    }

    [Fact]
    public void RenderWithHttpContext_ParameterBuilder_PassesComponentParameters()
    {
        var cut = this.RenderWithHttpContext<StatusMessage>(parameterBuilder: p => p
            .Add(x => x.Message, "Din adgangskode er ændret."));

        cut.Markup.Should().Contain("Din adgangskode er ændret.");
    }

    [Fact]
    public void RenderWithHttpContext_ExplicitHttpContext_CascadesThatInstance()
    {
        var cut = this.RenderWithHttpContext<Error>(new DefaultHttpContext { TraceIdentifier = "trace-xyz" });

        cut.Markup.Should().Contain("trace-xyz");
    }
}
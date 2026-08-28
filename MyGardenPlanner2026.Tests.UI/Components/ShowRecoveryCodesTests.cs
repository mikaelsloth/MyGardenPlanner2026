namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;

public class ShowRecoveryCodesTests : BunitContext
{
    [Fact]
    public void ShowRecoveryCodes_RendersAllCodesAsMonospaceElements()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<ShowRecoveryCodes>(parameters => parameters
            .AddCascadingValue(httpContext)
            .Add(p => p.RecoveryCodes, ["ABCD-1234", "EFGH-5678"]));

        cut.FindAll("code.recovery-code").Should().HaveCount(2);
        cut.Markup.Should().Contain("ABCD-1234");
        cut.Markup.Should().Contain("EFGH-5678");
    }

    [Fact]
    public void ShowRecoveryCodes_RendersWarningStatusMessage()
    {
        var httpContext = new DefaultHttpContext();
        var cut = Render<ShowRecoveryCodes>(parameters => parameters
            .AddCascadingValue(httpContext)
            .Add(p => p.RecoveryCodes, ["ABCD-1234"]));

        cut.Find(".status-warning").Should().NotBeNull();
    }
}
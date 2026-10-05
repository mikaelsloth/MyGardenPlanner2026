namespace MyGardenPlanner2026.Tests.UI.Components.Account.Shared;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Account.Shared;
using Xunit;

public class ShowRecoveryCodesTests : BunitContext
{
    [Fact]
    public void ShowRecoveryCodes_RendersAllCodesAsMonospaceElements()
    {
        var cut = this.RenderWithHttpContext<ShowRecoveryCodes>(parameterBuilder: p => p
            .Add(x => x.RecoveryCodes, ["ABCD-1234", "EFGH-5678"]));

        cut.FindAll("code.recovery-code").Should().HaveCount(2);
        cut.Markup.Should().Contain("ABCD-1234");
        cut.Markup.Should().Contain("EFGH-5678");
    }

    [Fact]
    public void ShowRecoveryCodes_RendersWarningStatusMessage()
    {
        var cut = this.RenderWithHttpContext<ShowRecoveryCodes>(parameterBuilder: p => p
            .Add(x => x.RecoveryCodes, ["ABCD-1234"]));

        cut.Find(".status-warning").Should().NotBeNull();
    }
}
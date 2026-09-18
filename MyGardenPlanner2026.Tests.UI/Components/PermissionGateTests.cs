namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class PermissionGateTests : BunitContext
{
    [Fact]
    public void Authorized_RendersChildContentDirectly_WithoutWrapperDiv()
    {
        var cut = Render<PermissionGate>(p => p
            .Add(g => g.IsAuthorized, true)
            .AddChildContent("<button class=\"btn btn-primary\">Opret bed</button>"));

        cut.FindAll(".permission-gate-disabled").Should().BeEmpty();
        cut.Find(".btn-primary").Should().NotBeNull();
    }

    [Fact]
    public void Unauthorized_DisableMode_WrapsChildContent_WithAriaDisabled()
    {
        var cut = Render<PermissionGate>(p => p
            .Add(g => g.IsAuthorized, false)
            .Add(g => g.Mode, PermissionGate.PermissionGateMode.Disable)
            .AddChildContent("<button class=\"btn btn-primary\">Opret bed</button>"));

        var wrapper = cut.Find(".permission-gate-disabled");
        wrapper.GetAttribute("aria-disabled").Should().Be("true");
        wrapper.QuerySelector(".btn-primary").Should().NotBeNull();
    }

    [Fact]
    public void Unauthorized_DisableMode_WithHintText_RendersPermissionHint()
    {
        var cut = Render<PermissionGate>(p => p
            .Add(g => g.IsAuthorized, false)
            .Add(g => g.HintText, "Tilkøb abonnement for at oprette ubegrænsede bede")
            .AddChildContent("<button class=\"btn btn-primary\">Opret bed</button>"));

        cut.Find(".permission-hint").TextContent.Should().Be("Tilkøb abonnement for at oprette ubegrænsede bede");
    }

    [Fact]
    public void Unauthorized_DisableMode_WithoutHintText_DoesNotRenderPermissionHint()
    {
        var cut = Render<PermissionGate>(p => p
            .Add(g => g.IsAuthorized, false)
            .AddChildContent("<button class=\"btn btn-primary\">Opret bed</button>"));

        cut.FindAll(".permission-hint").Should().BeEmpty();
    }

    [Fact]
    public void Unauthorized_HideMode_RendersNothing()
    {
        var cut = Render<PermissionGate>(p => p
            .Add(g => g.IsAuthorized, false)
            .Add(g => g.Mode, PermissionGate.PermissionGateMode.Hide)
            .Add(g => g.HintText, "Skal ikke vises")
            .AddChildContent("<button class=\"btn btn-primary\">Opret bed</button>"));

        cut.Markup.Should().BeEmpty();
    }
}
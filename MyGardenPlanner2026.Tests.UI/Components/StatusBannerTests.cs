namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class StatusBannerTests : BunitContext
{
    [Fact]
    public void DefaultVariant_RendersInfoClassAndStatusRole()
    {
        var cut = Render<StatusBanner>(p => p.AddChildContent("Systemet er under vedligehold."));

        cut.Find(".status-banner").ClassList.Should().Contain("status-banner-info");
        cut.Find(".status-banner").GetAttribute("role").Should().Be("status");
        cut.Markup.Should().Contain("Systemet er under vedligehold.");
    }

    [Fact]
    public void DangerVariant_RendersDangerClassAndAlertRole()
    {
        var cut = Render<StatusBanner>(p => p.Add(b => b.Variant, StatusBanner.StatusBannerVariant.Danger));

        cut.Find(".status-banner").ClassList.Should().Contain("status-banner-danger");
        cut.Find(".status-banner").GetAttribute("role").Should().Be("alert");
    }

    [Fact]
    public void NotDismissible_DoesNotRenderDismissButton()
    {
        var cut = Render<StatusBanner>();

        cut.FindAll(".status-banner-dismiss").Should().BeEmpty();
    }

    [Fact]
    public void Dismissible_ClickingDismiss_InvokesOnDismiss()
    {
        var dismissed = false;
        var cut = Render<StatusBanner>(p => p
            .Add(b => b.Dismissible, true)
            .Add(b => b.OnDismiss, () => dismissed = true));

        cut.Find(".status-banner-dismiss").Click();

        dismissed.Should().BeTrue();
    }

    [Fact]
    public void IconClass_RendersIconElement()
    {
        var cut = Render<StatusBanner>(p => p.Add(b => b.IconClass, "bi-info-circle"));

        cut.Find(".status-banner-content i").ClassList.Should().Contain("bi-info-circle");
    }
}
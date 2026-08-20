namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public class TermsPageTests : BunitContext
{
    [Fact]
    public void TermsPage_RendersImageUploadRestrictions()
    {
        var cut = Render<TermsPage>();

        cut.Markup.Should().Contain("kun uploade billeder");
        cut.Markup.Should().Contain("strengt forbudt");
    }

    [Fact]
    public void TermsPage_RendersLastUpdatedDate()
    {
        var cut = Render<TermsPage>();

        cut.Markup.Should().Contain("20. august 2026");
    }
}
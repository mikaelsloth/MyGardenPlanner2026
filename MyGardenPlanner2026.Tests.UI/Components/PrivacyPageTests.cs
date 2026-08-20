namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Pages;
using Xunit;

public class PrivacyPageTests : BunitContext
{
    [Fact]
    public void PrivacyPage_MentionsCookiesLimitedToCriticalFunctionality()
    {
        var cut = Render<PrivacyPage>();

        cut.Markup.Should().Contain("strengt nødvendige for tjenestens kritiske funktionalitet");
    }

    [Fact]
    public void PrivacyPage_MentionsNoDataSharingWithThirdParties()
    {
        var cut = Render<PrivacyPage>();

        cut.Markup.Should().Contain("Vi deler ikke dine oplysninger med tredjeparter.");
    }

    [Fact]
    public void PrivacyPage_ListsGdprComplaintRightToDatatilsynet()
    {
        var cut = Render<PrivacyPage>();

        cut.Markup.Should().Contain("Datatilsynet");
    }
}
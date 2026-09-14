namespace MyGardenPlanner2026.Tests.UI.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Services;
using Xunit;

public sealed class OnboardingStateContainerTests
{
    [Fact]
    public void NewInstance_HasAllFieldsAtDefaults()
    {
        var sut = new OnboardingStateContainer();

        sut.SelectedLayer.Should().BeNull();
        sut.SelectedCategory.Should().BeNull();
        sut.SelectedBillingCycle.Should().BeNull();
        sut.GardenName.Should().BeNull();
        sut.GardenDescription.Should().BeNull();
        sut.SelectedAddOnQuantities.Should().BeEmpty();
        sut.ActiveInvitationToken.Should().BeNull();
        sut.HasActiveInvitation.Should().BeFalse();
    }

    [Fact]
    public void HasActiveInvitation_TokenSet_ReturnsTrue()
    {
        var sut = new OnboardingStateContainer { ActiveInvitationToken = "raw-token" };

        sut.HasActiveInvitation.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasActiveInvitation_NullOrWhitespaceToken_ReturnsFalse(string? token)
    {
        var sut = new OnboardingStateContainer { ActiveInvitationToken = token };

        sut.HasActiveInvitation.Should().BeFalse();
    }

    [Fact]
    public void SelectedAddOnQuantities_CanAddAndReadQuantities()
    {
        var sut = new OnboardingStateContainer();
        var addOnId = Guid.NewGuid();

        sut.SelectedAddOnQuantities[addOnId] = 3;

        sut.SelectedAddOnQuantities[addOnId].Should().Be(3);
    }

    [Fact]
    public void Reset_ClearsAllFields()
    {
        var sut = new OnboardingStateContainer
        {
            SelectedLayer = GardenAccessLevel.BedDesigner,
            SelectedCategory = AccessCategory.Editor,
            SelectedBillingCycle = BillingCycle.Monthly,
            GardenName = "Min have",
            GardenDescription = "Beskrivelse",
            ActiveInvitationToken = "token"
        };
        sut.SelectedAddOnQuantities[Guid.NewGuid()] = 2;

        sut.Reset();

        sut.SelectedLayer.Should().BeNull();
        sut.SelectedCategory.Should().BeNull();
        sut.SelectedBillingCycle.Should().BeNull();
        sut.GardenName.Should().BeNull();
        sut.GardenDescription.Should().BeNull();
        sut.SelectedAddOnQuantities.Should().BeEmpty();
        sut.ActiveInvitationToken.Should().BeNull();
    }
}
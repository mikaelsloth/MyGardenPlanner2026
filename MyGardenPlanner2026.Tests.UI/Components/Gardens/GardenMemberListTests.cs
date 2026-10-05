namespace MyGardenPlanner2026.Tests.UI.Components.Gardens;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class GardenMemberListTests : BunitContext
{
    [Fact]
    public void RendersOneRowPerMember_WithRoleBadge()
    {
        var members = new List<GardenMembershipDto>
        {
            GardenTestData.Membership(userId: "owner-user"),
            GardenTestData.Membership(
                userId: "editor-user", isOwner: false,
                level: GardenAccessLevel.BedDesigner, category: AccessCategory.Editor)
        };

        var cut = Render<GardenMemberList>(p => p.Add(l => l.Members, members));

        cut.FindAll("tbody tr").Should().HaveCount(2);
        cut.Markup.Should().Contain("Ejer");
        cut.Markup.Should().Contain("Redaktør");
    }

    [Fact]
    public void EmptyMembers_RendersNoRows()
    {
        var cut = Render<GardenMemberList>(p => p.Add(l => l.Members, []));

        cut.FindAll("tbody tr").Should().BeEmpty();
    }
}
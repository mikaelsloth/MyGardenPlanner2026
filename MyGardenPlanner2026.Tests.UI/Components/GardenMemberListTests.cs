namespace MyGardenPlanner2026.Tests.UI.Components;

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
            new(Guid.NewGuid(), Guid.NewGuid(), "owner-user", true, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), Guid.NewGuid(), "editor-user", false, GardenAccessLevel.BedDesigner, AccessCategory.Editor, DateTimeOffset.UtcNow)
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
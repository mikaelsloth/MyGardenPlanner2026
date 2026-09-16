namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class GardenInvitationListTests : BunitContext
{
    private static GardenInvitationDto CreateInvitation(
        string invitedBy = "owner", bool isAccepted = false, bool isRevoked = false,
        DateTimeOffset? expiresUtc = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), invitedBy, "invited@example.com",
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, false, false,
            expiresUtc ?? DateTimeOffset.UtcNow.AddDays(7), isAccepted, isRevoked, DateTimeOffset.UtcNow);

    [Fact]
    public void GroupsInvitationsIntoCorrectSections()
    {
        var invitations = new List<GardenInvitationDto>
        {
            CreateInvitation(),
            CreateInvitation(isAccepted: true),
            CreateInvitation(expiresUtc: DateTimeOffset.UtcNow.AddDays(-1))
        };

        var cut = Render<GardenInvitationList>(p => p
            .Add(l => l.Invitations, invitations)
            .Add(l => l.CanManageAll, true)
            .Add(l => l.CurrentUserId, "owner"));

        cut.Markup.Should().Contain("Afventende (1)");
        cut.Markup.Should().Contain("Accepteret (1)");
        cut.Markup.Should().Contain("Udløbet / Tilbagekaldt (1)");
    }

    [Fact]
    public void CanManageAllFalse_OnlyShowsOwnInvitations()
    {
        var invitations = new List<GardenInvitationDto>
        {
            CreateInvitation(invitedBy: "user-1"),
            CreateInvitation(invitedBy: "user-2")
        };

        var cut = Render<GardenInvitationList>(p => p
            .Add(l => l.Invitations, invitations)
            .Add(l => l.CanManageAll, false)
            .Add(l => l.CurrentUserId, "user-1"));

        cut.Markup.Should().Contain("Afventende (1)");
    }

    [Fact]
    public void ClickingRevoke_OpensConfirmDialog_WithoutInvokingOnRevokeYet()
    {
        var invoked = false;
        var invitations = new List<GardenInvitationDto> { CreateInvitation() };

        var cut = Render<GardenInvitationList>(p => p
            .Add(l => l.Invitations, invitations)
            .Add(l => l.CanManageAll, true)
            .Add(l => l.CurrentUserId, "owner")
            .Add(l => l.OnRevoke, _ => invoked = true));

        cut.Find(".btn-danger").Click();

        cut.Find(".confirm-dialog").Should().NotBeNull();
        invoked.Should().BeFalse();
    }

    [Fact]
    public void ConfirmingRevokeDialog_InvokesOnRevokeWithCorrectId()
    {
        Guid? revokedId = null;
        var invitation = CreateInvitation();

        var cut = Render<GardenInvitationList>(p => p
            .Add(l => l.Invitations, [invitation])
            .Add(l => l.CanManageAll, true)
            .Add(l => l.CurrentUserId, "owner")
            .Add(l => l.OnRevoke, id => revokedId = id));

        cut.Find(".btn-danger").Click();
        cut.Find(".confirm-dialog-actions .btn-danger").Click();

        revokedId.Should().Be(invitation.Id);
    }

    [Fact]
    public void CancellingRevokeDialog_ClosesDialog_WithoutInvokingOnRevoke()
    {
        var invoked = false;
        var invitations = new List<GardenInvitationDto> { CreateInvitation() };

        var cut = Render<GardenInvitationList>(p => p
            .Add(l => l.Invitations, invitations)
            .Add(l => l.CanManageAll, true)
            .Add(l => l.CurrentUserId, "owner")
            .Add(l => l.OnRevoke, _ => invoked = true));

        cut.Find(".btn-danger").Click();
        cut.Find(".confirm-dialog-actions .btn-secondary").Click();

        cut.FindAll(".confirm-dialog").Should().BeEmpty();
        invoked.Should().BeFalse();
    }
}
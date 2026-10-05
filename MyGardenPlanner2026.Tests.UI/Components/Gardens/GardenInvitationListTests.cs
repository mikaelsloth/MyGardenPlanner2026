namespace MyGardenPlanner2026.Tests.UI.Components.Gardens;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using Xunit;

public sealed class GardenInvitationListTests : BunitContext
{
    [Fact]
    public void GroupsInvitationsIntoCorrectSections()
    {
        var invitations = new List<GardenInvitationDto>
        {
            GardenTestData.Invitation(),
            GardenTestData.Invitation(isAccepted: true),
            GardenTestData.Invitation(expiresUtc: DateTimeOffset.UtcNow.AddDays(-1))
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
            GardenTestData.Invitation(invitedByUserId: "user-1"),
            GardenTestData.Invitation(invitedByUserId: "user-2")
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
        var invitations = new List<GardenInvitationDto> { GardenTestData.Invitation() };

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
        var invitation = GardenTestData.Invitation();

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
        var invitations = new List<GardenInvitationDto> { GardenTestData.Invitation() };

        var cut = Render<GardenInvitationList>(p => p
            .Add(l => l.Invitations, invitations)
            .Add(l => l.CanManageAll, true)
            .Add(l => l.CurrentUserId, "owner")
            .Add(l => l.OnRevoke, _ => invoked = true));

        cut.Find(".btn-danger").Click();
        cut.Find(".confirm-dialog-actions .btn-secondary").Click();

        cut.ShouldNotShowStepUpModal();
        invoked.Should().BeFalse();
    }
}
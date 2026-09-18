namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class InvitationStatusCardTests : BunitContext
{
    private static GardenInvitationDto CreateInvitation(
        bool isAccepted = false, bool isRevoked = false, DateTimeOffset? expiresUtc = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "user-id", "invited@example.com",
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, false, false,
            expiresUtc ?? DateTimeOffset.UtcNow.AddDays(7), isAccepted, isRevoked, DateTimeOffset.UtcNow,
            GardenAccessLevel.BedDesigner, AccessCategory.Editor);

    [Fact]
    public void PendingInvitation_ShowsAfventerBadge()
    {
        var cut = Render<InvitationStatusCard>(p => p.Add(c => c.Invitation, CreateInvitation()));

        cut.Markup.Should().Contain("Afventer");
    }

    [Fact]
    public void AcceptedInvitation_ShowsAccepteretBadge()
    {
        var cut = Render<InvitationStatusCard>(p => p.Add(c => c.Invitation, CreateInvitation(isAccepted: true)));

        cut.Markup.Should().Contain("Accepteret");
    }

    [Fact]
    public void ExpiredInvitation_ShowsUdloebetBadge()
    {
        var cut = Render<InvitationStatusCard>(p => p
            .Add(c => c.Invitation, CreateInvitation(expiresUtc: DateTimeOffset.UtcNow.AddDays(-1))));

        cut.Markup.Should().Contain("Udløbet");
    }

    [Fact]
    public void PendingInvitation_AllowRevokeTrue_ClickingRevoke_InvokesOnRevokeWithId()
    {
        Guid? revokedId = null;
        var invitation = CreateInvitation();
        var cut = Render<InvitationStatusCard>(p => p
            .Add(c => c.Invitation, invitation)
            .Add(c => c.AllowRevoke, true)
            .Add(c => c.OnRevoke, id => revokedId = id));

        cut.Find(".btn-danger").Click();

        revokedId.Should().Be(invitation.Id);
    }

    [Fact]
    public void AcceptedInvitation_AllowRevokeTrue_DoesNotRenderRevokeButton()
    {
        var cut = Render<InvitationStatusCard>(p => p
            .Add(c => c.Invitation, CreateInvitation(isAccepted: true))
            .Add(c => c.AllowRevoke, true));

        cut.FindAll(".btn-danger").Should().BeEmpty();
    }
}
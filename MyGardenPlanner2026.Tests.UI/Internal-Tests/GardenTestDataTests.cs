namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class GardenTestDataTests
{
    [Fact]
    public void Membership_Defaults_AreOwnerAdministratorOfHaveArkitektForDefaultUser()
    {
        var (_, _, userId, isOwner, level, category, _) = GardenTestData.Membership();

        userId.Should().Be("user-1");
        isOwner.Should().BeTrue();
        level.Should().Be(GardenAccessLevel.HaveArkitekt);
        category.Should().Be(AccessCategory.Administrator);
    }

    [Fact]
    public void Membership_Overrides_AreApplied()
    {
        var gardenId = Guid.NewGuid();

        var (_, actualGardenId, userId, isOwner, level, category, _) = GardenTestData.Membership(
            gardenId, "editor-user", isOwner: false, GardenAccessLevel.BedDesigner, AccessCategory.Editor);

        actualGardenId.Should().Be(gardenId);
        userId.Should().Be("editor-user");
        isOwner.Should().BeFalse();
        level.Should().Be(GardenAccessLevel.BedDesigner);
        category.Should().Be(AccessCategory.Editor);
    }

    [Fact]
    public void Invitation_Defaults_ArePendingWithCeilingEqualToTarget()
    {
        var (_, _, invitedBy, email, targetLayer, targetCategory, _, allowSelfUpgrade, expiresUtc,
            isAccepted, isRevoked, _, maxLayer, maxCategory) = GardenTestData.Invitation();

        invitedBy.Should().Be("owner");
        email.Should().Be("invited@example.com");
        targetLayer.Should().Be(GardenAccessLevel.BedDesigner);
        targetCategory.Should().Be(AccessCategory.Editor);
        allowSelfUpgrade.Should().BeFalse();
        expiresUtc.Should().BeAfter(DateTimeOffset.UtcNow);
        isAccepted.Should().BeFalse();
        isRevoked.Should().BeFalse();
        maxLayer.Should().Be(targetLayer);
        maxCategory.Should().Be(targetCategory);
    }

    [Fact]
    public void Invitation_Overrides_AreApplied()
    {
        var id = Guid.NewGuid();
        var gardenId = Guid.NewGuid();
        var expires = DateTimeOffset.UtcNow.AddDays(-1);

        var (actualId, actualGardenId, _, _, targetLayer, _, _, allowSelfUpgrade, expiresUtc,
            isAccepted, isRevoked, _, maxLayer, maxCategory) = GardenTestData.Invitation(
                id, gardenId, targetLayer: GardenAccessLevel.Planlaegger, allowSelfUpgrade: true,
                expiresUtc: expires, isAccepted: true, isRevoked: true,
                maxLayer: GardenAccessLevel.BedDesigner, maxCategory: AccessCategory.Administrator);

        actualId.Should().Be(id);
        actualGardenId.Should().Be(gardenId);
        targetLayer.Should().Be(GardenAccessLevel.Planlaegger);
        allowSelfUpgrade.Should().BeTrue();
        expiresUtc.Should().Be(expires);
        isAccepted.Should().BeTrue();
        isRevoked.Should().BeTrue();
        maxLayer.Should().Be(GardenAccessLevel.BedDesigner);
        maxCategory.Should().Be(AccessCategory.Administrator);
    }

    [Fact]
    public void Summary_UsesGivenIdAndDefaultName()
    {
        var gardenId = Guid.NewGuid();

        var (id, name, _) = GardenTestData.Summary(gardenId);

        id.Should().Be(gardenId);
        name.Should().Be("Testhave");
    }
}
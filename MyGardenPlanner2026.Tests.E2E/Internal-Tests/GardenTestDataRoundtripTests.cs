namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

[Collection(PlaywrightAppCollection.Name)]
public sealed class GardenTestDataRoundtripTests(PlaywrightAppFixture fixture)
{
    private string ConnectionString => fixture.AppConnectionString;

    [Fact]
    public async Task SeedExtraUserAsync_WithoutRoleAnd2fa_CreatesPlainUser()
    {
        var user = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);

        user.Role.Should().BeNull();
        user.TwoFactorEnabled.Should().BeFalse();
        user.AuthenticatorKey.Should().BeNull();
        (await SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, user.Email)).Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SeedAsync_OwnerAndMember_AreReadBackWithOwnershipRolesAndOwnerEntitlement()
    {
        var owner = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var member = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var ownerId = await SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, owner.Email);
        var memberId = await SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, member.Email);

        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            $"Roundtrip {Guid.NewGuid():N}",
            [GardenMemberSeed.Owner(owner.Email), GardenMemberSeed.Member(member.Email, AccessCategory.Viewer)]));

        var found = await GardenTestData.FindGardenAsync(ConnectionString, garden.Id);
        found.Should().NotBeNull();
        found!.Name.Should().Be(garden.Name);
        found.IsDeleted.Should().BeFalse();
        found.DeletedByUserId.Should().BeNull();

        var memberships = await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id);
        memberships.Should().BeEquivalentTo(
        [
            new MembershipSnapshot(ownerId, true, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, false),
            new MembershipSnapshot(memberId, false, GardenAccessLevel.HaveArkitekt, AccessCategory.Viewer, false),
        ]);

        var entitlements = await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id);
        entitlements.Should().BeEquivalentTo(
        [
            new EntitlementSnapshot(
                ownerId, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, BillingCycle.Annual, false, false),
        ]);
    }

    [Fact]
    public async Task SeedAsync_SoleOwner_GivesOneMembership()
    {
        var owner = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);

        var garden = await GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed($"Alene {Guid.NewGuid():N}", [GardenMemberSeed.Owner(owner.Email)]));

        var memberships = await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id);
        memberships.Should().ContainSingle().Which.IsOwner.Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_MemberWithEntitlement_GetsOwnEntitlement()
    {
        var owner = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var member = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);

        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            $"Medlem {Guid.NewGuid():N}",
            [
                GardenMemberSeed.Owner(owner.Email),
                GardenMemberSeed.Member(member.Email, AccessCategory.Editor, withEntitlement: true),
            ]));

        var entitlements = await GardenTestData.GetEntitlementsAsync(ConnectionString, garden.Id);
        entitlements.Should().HaveCount(2);
    }

    [Fact]
    public async Task SeedAsync_UnknownUser_Throws()
    {
        var act = () => GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed("Ukendt", [GardenMemberSeed.Owner("findes-ikke@test.dk")]));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*findes-ikke@test.dk*");
    }

    [Fact]
    public async Task FindGardenAsync_UnknownId_ReturnsNull() =>
        (await GardenTestData.FindGardenAsync(ConnectionString, Guid.NewGuid())).Should().BeNull();

    [Fact]
    public async Task SeedAsync_TwoOwners_BothAreOwners()
    {
        var first = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var second = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var firstId = await SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, first.Email);
        var secondId = await SmokeTestDataSeeder.GetUserIdAsync(ConnectionString, second.Email);

        var garden = await GardenTestData.SeedAsync(ConnectionString, new GardenSeed(
            $"To ejere {Guid.NewGuid():N}",
            [GardenMemberSeed.Owner(first.Email), GardenMemberSeed.Owner(second.Email)]));

        var memberships = await GardenTestData.GetMembershipsAsync(ConnectionString, garden.Id);
        memberships.Select(m => m.UserId).Should().BeEquivalentTo([firstId, secondId]);
        memberships.Should().OnlyContain(m => m.IsOwner);
    }

    [Fact]
    public async Task FindPendingRestoreAsync_GardenWithoutTicket_ReturnsNull()
    {
        var owner = await fixture.SeedExtraUserAsync(twoFactorEnabled: false);
        var garden = await GardenTestData.SeedAsync(ConnectionString,
            new GardenSeed($"Uden billet {Guid.NewGuid():N}", [GardenMemberSeed.Owner(owner.Email)]));

        (await GardenTestData.FindPendingRestoreAsync(ConnectionString, garden.Id)).Should().BeNull();
    }
}
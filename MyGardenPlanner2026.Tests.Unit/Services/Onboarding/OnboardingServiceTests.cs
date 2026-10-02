namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

public sealed class OnboardingServiceTests : OnboardingTestDbContext
{
    [Fact]
    public async Task CreateSandboxGardenAsync_CreatesOwnerMembershipAndTrialEntitlement()
    {
        var sut = CreateOnboardingService();

        var result = await sut.CreateSandboxGardenAsync("user-1", TestContext.Current.CancellationToken);

        await using var context = CreateDbContext();
        var membership = context.GardenMemberships.Single(m => m.Id == result.MembershipId);
        var entitlement = context.UserEntitlements.Single(e => e.Id == result.EntitlementId);

        membership.IsOwner.Should().BeTrue();
        membership.Layer.Should().Be(GardenAccessLevel.HaveArkitekt);
        membership.Category.Should().Be(AccessCategory.Administrator);
        entitlement.IsTrial.Should().BeTrue();
    }

    [Fact]
    public async Task ProvisionPaidGardenAsync_PerpetualBillingCycle_HasNullValidTo()
    {
        var sut = CreateOnboardingService();

        var request = new PaidGardenProvisionRequestDto(
            "user-1", "Min have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Perpetual, new Dictionary<Guid, int>());

        var result = await sut.ProvisionPaidGardenAsync(request, TestContext.Current.CancellationToken);

        await using var context = CreateDbContext();
        var entitlement = context.UserEntitlements.Single(e => e.Id == result.EntitlementId);

        entitlement.ValidToUtc.Should().BeNull();
        entitlement.IsTrial.Should().BeFalse();
    }

    [Fact]
    public async Task ProvisionPaidGardenAsync_AnnualBillingCycle_SetsValidToOneYearAhead()
    {
        var sut = CreateOnboardingService();

        var request = new PaidGardenProvisionRequestDto(
            "user-1", "Min have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>());

        var result = await sut.ProvisionPaidGardenAsync(request, TestContext.Current.CancellationToken);

        await using var context = CreateDbContext();
        var entitlement = context.UserEntitlements.Single(e => e.Id == result.EntitlementId);

        entitlement.ValidToUtc.Should().Be(FixedNow.AddYears(1));
    }

    [Fact]
    public async Task ProvisionPaidGardenAsync_WithAddOnQuantities_IncrementsCorrectQuotaFields()
    {
        var addOn = TestEntities.AddOn(annualPrice: 1m, monthlyPrice: 1m, perpetualPrice: 1m);
        await using (var context = CreateDbContext())
        {
            await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateOnboardingService();
        var request = new PaidGardenProvisionRequestDto(
            "user-1", "Min have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int> { [addOn.Id] = 3 });

        var result = await sut.ProvisionPaidGardenAsync(request, TestContext.Current.CancellationToken);

        await using var verifyContext = CreateDbContext();
        verifyContext.UserEntitlements.Single(e => e.Id == result.EntitlementId)
            .ExtraCategoryAArtifactsCount.Should().Be(3);
    }

    [Fact]
    public async Task CreateInvitationAsync_RequesterNotMember_ThrowsInvalidOperationException()
    {
        var garden = await SeedGardenAsync();
        var sut = CreateOnboardingService();
        var request = InvitationRequest(
            garden.Id, "not-a-member", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var act = () => sut.CreateInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateInvitationAsync_TargetLayerBetterThanRequester_ThrowsInvalidOperationException()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "owner", GardenAccessLevel.Planlaegger, AccessCategory.Editor, isOwner: true);
        var sut = CreateOnboardingService();
        var request = InvitationRequest(
            garden.Id, "owner", GardenAccessLevel.HaveArkitekt, AccessCategory.Editor);

        var act = () => sut.CreateInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateInvitationAsync_ValidRequest_TokenHashMatchesGeneratedRawToken()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        var sut = CreateOnboardingService();
        var request = InvitationRequest(
            garden.Id, "owner", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var result = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        await using var verifyContext = CreateDbContext();
        var invitation = verifyContext.GardenInvitations.Single(i => i.Id == result.InvitationId);

        invitation.TokenHash.Should().Be(new InvitationTokenService().HashToken(result.RawToken));
    }

    [Fact]
    public async Task CreateInvitationAsync_UseFreeSlotWithMismatchedLayer_ThrowsInvalidOperationException()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        await SeedEntitlementAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, BillingCycle.Annual);
        var sut = CreateOnboardingService();
        var request = InvitationRequest(
            garden.Id, "owner", GardenAccessLevel.Planlaegger, AccessCategory.Viewer, useFreeSlot: true);

        var act = () => sut.CreateInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateInvitationAsync_UseFreeSlotWithAvailableQuota_SetsIsFreeSlotTrue()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        await SeedEntitlementAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, BillingCycle.Annual);
        var sut = CreateOnboardingService();
        var request = InvitationRequest(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, useFreeSlot: true);

        var result = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        result.IsFreeSlot.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateInvitationTokenAsync_ValidToken_ReturnsIsValidTrue()
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, isOwner: true);
        var sut = CreateOnboardingService();
        var request = InvitationRequest(
            garden.Id, "owner", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        var created = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        var result = await sut.ValidateInvitationTokenAsync(created.RawToken, TestContext.Current.CancellationToken);

        result.IsValid.Should().BeTrue();
        result.Invitation!.Email.Should().Be("invited@example.com");
    }

    [Fact]
    public async Task ValidateInvitationTokenAsync_UnknownToken_ReturnsIsValidFalse()
    {
        var sut = CreateOnboardingService();

        var result = await sut.ValidateInvitationTokenAsync("does-not-exist", TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RevokeInvitationAsync_PendingInvitation_SetsIsRevokedTrue()
    {
        var garden = await SeedGardenAsync();
        var invitation = await SeedInvitationAsync(garden.Id);
        var sut = CreateOnboardingService();

        await sut.RevokeInvitationAsync(invitation.Id, "owner", TestContext.Current.CancellationToken);

        await using var verifyContext = CreateDbContext();
        verifyContext.GardenInvitations.Single(i => i.Id == invitation.Id).IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeInvitationAsync_AcceptedInvitation_ThrowsInvalidOperationException()
    {
        var garden = await SeedGardenAsync();
        var invitation = await SeedInvitationAsync(garden.Id, isAccepted: true);
        var sut = CreateOnboardingService();

        var act = () => sut.RevokeInvitationAsync(invitation.Id, "owner");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetFreeInvitationQuotaAsync_RevokedInvitationDoesNotCountAsUsed()
    {
        var garden = await SeedGardenAsync();
        await SeedEntitlementAsync(
            garden.Id, "owner", GardenAccessLevel.BedDesigner, AccessCategory.Editor, BillingCycle.Annual);
        await SeedInvitationAsync(garden.Id, email: "a@example.com", isFreeSlot: true, isRevoked: true);
        var sut = CreateOnboardingService();

        var quota = await sut.GetFreeInvitationQuotaAsync(garden.Id, "owner", TestContext.Current.CancellationToken);

        quota.TotalFreeSlots.Should().Be(1);
        quota.UsedFreeSlots.Should().Be(0);
        quota.RemainingFreeSlots.Should().Be(1);
    }
}
namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

/// <summary>
/// Dækker CheckoutDraft-metoderne og AcceptInvitationAsync. Egen fil for ikke at kollidere
/// med OnboardingServiceTests.cs.
/// </summary>
public sealed class OnboardingCheckoutAndInvitationTests : OnboardingTestDbContext
{
    private static SaveCheckoutDraftRequestDto DraftRequest(
        string? description = null,
        GardenAccessLevel layer = GardenAccessLevel.BedDesigner,
        AccessCategory category = AccessCategory.Editor,
        BillingCycle billingCycle = BillingCycle.Annual) =>
        new(null, "Min have", description, layer, category, billingCycle, new Dictionary<Guid, int>());

    private static AcceptInvitationRequestDto AcceptRequest(
        string rawToken,
        GardenAccessLevel layer = GardenAccessLevel.Planlaegger,
        AccessCategory category = AccessCategory.Viewer,
        BillingCycle? upgradeBillingCycle = null) =>
        new(rawToken, "invited-user", layer, category,
            UpgradeBillingCycle: upgradeBillingCycle, AddOnQuantities: new Dictionary<Guid, int>());

    private async Task<(Guid GardenId, string RawToken)> CreateInvitationAsync(
        OnboardingService sut,
        GardenAccessLevel targetLayer, AccessCategory targetCategory,
        GardenAccessLevel maxLayer, AccessCategory maxCategory, bool allowSelfUpgrade = false,
        GardenAccessLevel ownerLayer = GardenAccessLevel.HaveArkitekt,
        AccessCategory ownerCategory = AccessCategory.Administrator)
    {
        var garden = await SeedGardenAsync();
        await SeedMembershipAsync(garden.Id, "owner", ownerLayer, ownerCategory, isOwner: true);

        var request = InvitationRequest(
            garden.Id, "owner", targetLayer, targetCategory, maxLayer, maxCategory, allowSelfUpgrade);
        var created = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        return (garden.Id, created.RawToken);
    }

    [Fact]
    public async Task SaveCheckoutDraftAsync_ThenGetCheckoutDraftAsync_ReturnsMatchingValues()
    {
        var sut = CreateOnboardingService();

        var draftId = await sut.SaveCheckoutDraftAsync(DraftRequest("Beskrivelse"), TestContext.Current.CancellationToken);
        var draft = await sut.GetCheckoutDraftAsync(draftId, TestContext.Current.CancellationToken);

        draft.Should().NotBeNull();
        draft!.GardenName.Should().Be("Min have");
        draft.Layer.Should().Be(GardenAccessLevel.BedDesigner);
        draft.Category.Should().Be(AccessCategory.Editor);
    }

    [Fact]
    public async Task GetCheckoutDraftAsync_UnknownId_ReturnsNull()
    {
        var sut = CreateOnboardingService();

        var draft = await sut.GetCheckoutDraftAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        draft.Should().BeNull();
    }

    [Fact]
    public async Task GetCheckoutDraftAsync_ExpiredDraft_ReturnsNull()
    {
        var timeProvider = new TestTimeProvider(FixedNow);
        var sut = CreateOnboardingService(timeProvider);
        var request = DraftRequest(
            layer: GardenAccessLevel.Planlaegger, category: AccessCategory.Viewer, billingCycle: BillingCycle.Monthly);
        var draftId = await sut.SaveCheckoutDraftAsync(request, TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromHours(1));

        var draft = await sut.GetCheckoutDraftAsync(draftId, TestContext.Current.CancellationToken);

        draft.Should().BeNull();
    }

    [Fact]
    public async Task ProvisionPaidGardenFromDraftAsync_ValidDraft_CreatesGardenMembershipAndEntitlement_AndDeletesDraft()
    {
        var sut = CreateOnboardingService();
        var draftId = await sut.SaveCheckoutDraftAsync(DraftRequest(), TestContext.Current.CancellationToken);

        var result = await sut.ProvisionPaidGardenFromDraftAsync(draftId, "user-1", TestContext.Current.CancellationToken);

        using var context = CreateDbContext();
        (await context.Gardens.SingleAsync(g => g.Id == result.GardenId, TestContext.Current.CancellationToken)).Name.Should().Be("Min have");
        (await context.GardenMemberships.SingleAsync(m => m.Id == result.MembershipId, TestContext.Current.CancellationToken)).IsOwner.Should().BeTrue();
        (await context.UserEntitlements.SingleAsync(e => e.Id == result.EntitlementId, TestContext.Current.CancellationToken)).Layer.Should().Be(GardenAccessLevel.BedDesigner);
        (await context.CheckoutDrafts.AnyAsync(d => d.Id == draftId, TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task ProvisionPaidGardenFromDraftAsync_UnknownDraft_ThrowsInvalidOperationException()
    {
        var sut = CreateOnboardingService();

        var act = () => sut.ProvisionPaidGardenFromDraftAsync(Guid.NewGuid(), "user-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProvisionPaidGardenFromDraftAsync_ExpiredDraft_ThrowsInvalidOperationException_AndDeletesDraft()
    {
        var timeProvider = new TestTimeProvider(FixedNow);
        var sut = CreateOnboardingService(timeProvider);
        var draftId = await sut.SaveCheckoutDraftAsync(DraftRequest(), TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromHours(1));

        var act = () => sut.ProvisionPaidGardenFromDraftAsync(draftId, "user-1");

        await act.Should().ThrowAsync<InvalidOperationException>();

        using var context = CreateDbContext();
        (await context.CheckoutDrafts.AnyAsync(d => d.Id == draftId, TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task AcceptInvitationAsync_NoUpgrade_CreatesMembershipOnly_AndMarksAccepted()
    {
        var sut = CreateOnboardingService();
        var (gardenId, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var result = await sut.AcceptInvitationAsync(AcceptRequest(rawToken), TestContext.Current.CancellationToken);

        result.GardenId.Should().Be(gardenId);
        result.EntitlementId.Should().BeNull();

        var tokenHash = new InvitationTokenService().HashToken(rawToken);
        using var context = CreateDbContext();
        (await context.GardenMemberships.SingleAsync(m => m.Id == result.MembershipId, TestContext.Current.CancellationToken)).IsOwner.Should().BeFalse();
        (await context.GardenInvitations.SingleAsync(i => i.TokenHash == tokenHash, TestContext.Current.CancellationToken)).IsAccepted.Should().BeTrue();
    }

    [Fact]
    public async Task AcceptInvitationAsync_WithUpgrade_CreatesMembershipAndEntitlementAtGrantedLevel()
    {
        var sut = CreateOnboardingService();
        var (_, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, allowSelfUpgrade: true);
        var request = AcceptRequest(
            rawToken, GardenAccessLevel.BedDesigner, AccessCategory.Editor, BillingCycle.Annual);

        var result = await sut.AcceptInvitationAsync(request, TestContext.Current.CancellationToken);

        result.EntitlementId.Should().NotBeNull();

        using var context = CreateDbContext();
        var membership = await context.GardenMemberships.SingleAsync(m => m.Id == result.MembershipId, TestContext.Current.CancellationToken);
        membership.Layer.Should().Be(GardenAccessLevel.BedDesigner);
        membership.Category.Should().Be(AccessCategory.Editor);

        var entitlement = await context.UserEntitlements.SingleAsync(e => e.Id == result.EntitlementId, TestContext.Current.CancellationToken);
        entitlement.Layer.Should().Be(GardenAccessLevel.BedDesigner);
        entitlement.BillingCycle.Should().Be(BillingCycle.Annual);
    }

    [Fact]
    public async Task AcceptInvitationAsync_GrantedLayerBetterThanMaxAllowed_ThrowsInvalidOperationException()
    {
        var sut = CreateOnboardingService();

        // Ejeren har IKKE de bedst mulige rettigheder (BedDesigner/Editor, ikke
        // HaveArkitekt/Administrator) — så invitationens reelle loft (sat til ejerens
        // egne rettigheder, jf. CreateInvitationAsync når AllowSelfUpgrade er true)
        // bliver netop BedDesigner/Editor, og et forsøg på at acceptere med bedre
        // rettigheder end det skal afvises.
        var (_, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, allowSelfUpgrade: true,
            ownerLayer: GardenAccessLevel.BedDesigner, ownerCategory: AccessCategory.Editor);
        var request = AcceptRequest(
            rawToken, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, BillingCycle.Annual);

        var act = () => sut.AcceptInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_AlreadyAccepted_ThrowsInvalidOperationException()
    {
        var sut = CreateOnboardingService();
        var (_, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        var request = AcceptRequest(rawToken);
        await sut.AcceptInvitationAsync(request, TestContext.Current.CancellationToken);

        var act = () => sut.AcceptInvitationAsync(request with { UserId = "another-user" });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_RevokedInvitation_ThrowsInvalidOperationException()
    {
        var sut = CreateOnboardingService();
        var (_, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var tokenHash = new InvitationTokenService().HashToken(rawToken);
        using (var context = CreateDbContext())
        {
            var invitation = await context.GardenInvitations.SingleAsync(i => i.TokenHash == tokenHash, TestContext.Current.CancellationToken);
            invitation.IsRevoked = true;
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var act = () => sut.AcceptInvitationAsync(AcceptRequest(rawToken));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_ExpiredInvitation_ThrowsInvalidOperationException()
    {
        var timeProvider = new TestTimeProvider(FixedNow);
        var sut = CreateOnboardingService(timeProvider);
        var (_, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        timeProvider.Advance(TimeSpan.FromDays(8));

        var act = () => sut.AcceptInvitationAsync(AcceptRequest(rawToken));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_UserAlreadyMemberOfGarden_ThrowsInvalidOperationException()
    {
        var sut = CreateOnboardingService();
        var (gardenId, rawToken) = await CreateInvitationAsync(
            sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);
        await SeedMembershipAsync(
            gardenId, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var act = () => sut.AcceptInvitationAsync(AcceptRequest(rawToken));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
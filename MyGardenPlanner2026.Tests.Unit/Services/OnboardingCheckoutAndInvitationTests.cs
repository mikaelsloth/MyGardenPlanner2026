namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

/// <summary>
/// Dækker de 4 metoder tilføjet i Prompt 3 (CheckoutDraft + AcceptInvitationAsync).
/// Egen fil for ikke at kollidere med den eksisterende, lokalt rettede
/// OnboardingServiceTests.cs.
/// </summary>
public sealed class OnboardingCheckoutAndInvitationTests : TestDbContext
{
    private static readonly DateTimeOffset FixedNow = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static OnboardingService CreateSut(IDbContextFactory<PlannerDbContext> testContext, TestTimeProvider timeProvider) =>
        new(testContext, new InvitationTokenService(), timeProvider, NullLogger<OnboardingService>.Instance);

    [Fact]
    public async Task SaveCheckoutDraftAsync_ThenGetCheckoutDraftAsync_ReturnsMatchingValues()
    {
        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var request = new SaveCheckoutDraftRequestDto(
            null, "Min have", "Beskrivelse", GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>());

        var draftId = await sut.SaveCheckoutDraftAsync(request, TestContext.Current.CancellationToken);
        var draft = await sut.GetCheckoutDraftAsync(draftId, TestContext.Current.CancellationToken);

        draft.Should().NotBeNull();
        draft!.GardenName.Should().Be("Min have");
        draft.Layer.Should().Be(GardenAccessLevel.BedDesigner);
        draft.Category.Should().Be(AccessCategory.Editor);
    }

    [Fact]
    public async Task GetCheckoutDraftAsync_UnknownId_ReturnsNull()
    {
        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var draft = await sut.GetCheckoutDraftAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        draft.Should().BeNull();
    }

    [Fact]
    public async Task GetCheckoutDraftAsync_ExpiredDraft_ReturnsNull()
    {
        var timeProvider = new TestTimeProvider(FixedNow);
        var sut = CreateSut(CreateDbContextFactory(), timeProvider);

        var request = new SaveCheckoutDraftRequestDto(
            null, "Min have", null, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            BillingCycle.Monthly, new Dictionary<Guid, int>());
        var draftId = await sut.SaveCheckoutDraftAsync(request, TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromHours(1));

        var draft = await sut.GetCheckoutDraftAsync(draftId, TestContext.Current.CancellationToken);

        draft.Should().BeNull();
    }

    [Fact]
    public async Task ProvisionPaidGardenFromDraftAsync_ValidDraft_CreatesGardenMembershipAndEntitlement_AndDeletesDraft()
    {
        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var request = new SaveCheckoutDraftRequestDto(
            null, "Min have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>());
        var draftId = await sut.SaveCheckoutDraftAsync(request, TestContext.Current.CancellationToken);

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
        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var act = () => sut.ProvisionPaidGardenFromDraftAsync(Guid.NewGuid(), "user-1");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProvisionPaidGardenFromDraftAsync_ExpiredDraft_ThrowsInvalidOperationException_AndDeletesDraft()
    {
        var timeProvider = new TestTimeProvider(FixedNow);
        var sut = CreateSut(CreateDbContextFactory(), timeProvider);

        var request = new SaveCheckoutDraftRequestDto(
            null, "Min have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>());
        var draftId = await sut.SaveCheckoutDraftAsync(request, TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromHours(1));

        var act = () => sut.ProvisionPaidGardenFromDraftAsync(draftId, "user-1");

        await act.Should().ThrowAsync<InvalidOperationException>();

        using var context = CreateDbContext();
        (await context.CheckoutDrafts.AnyAsync(d => d.Id == draftId, TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    private static async Task<(Guid GardenId, string RawToken)> CreateInvitationAsync(
        IDbContextFactory<PlannerDbContext> testContext, OnboardingService sut,
        GardenAccessLevel targetLayer, AccessCategory targetCategory,
        GardenAccessLevel maxLayer, AccessCategory maxCategory, bool allowSelfUpgrade = false,
        GardenAccessLevel ownerLayer = GardenAccessLevel.HaveArkitekt,
        AccessCategory ownerCategory = AccessCategory.Administrator)
    {
        var garden = new Garden { Name = "Testhave" };
        var owner = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "owner",
            IsOwner = true,
            Layer = ownerLayer,
            Category = ownerCategory
        };
        await using var context = await testContext.CreateDbContextAsync();
        await context.Gardens.AddAsync(garden);
        await context.GardenMemberships.AddAsync(owner);
        await context.SaveChangesAsync();

        var request = new CreateInvitationRequestDto(
            garden.Id, "owner", "invited@example.com", targetLayer, targetCategory,
            maxLayer, maxCategory, AllowSelfUpgrade: allowSelfUpgrade, UseFreeSlot: false, ValidFor: TimeSpan.FromDays(7));
        var created = await sut.CreateInvitationAsync(request);

        return (garden.Id, created.RawToken);
    }

    [Fact]
    public async Task AcceptInvitationAsync_NoUpgrade_CreatesMembershipOnly_AndMarksAccepted()
    {
        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var (gardenId, rawToken) = await CreateInvitationAsync(
            CreateDbContextFactory(), sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            UpgradeBillingCycle: null, AddOnQuantities: new Dictionary<Guid, int>());

        var result = await sut.AcceptInvitationAsync(request, TestContext.Current.CancellationToken);

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

        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var (_, rawToken) = await CreateInvitationAsync(
            CreateDbContextFactory(), sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, allowSelfUpgrade: true);

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            UpgradeBillingCycle: BillingCycle.Annual, AddOnQuantities: new Dictionary<Guid, int>());

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
        var testContext = CreateDbContextFactory();
        var sut = CreateSut(testContext, new TestTimeProvider(FixedNow));

        // Ejeren har IKKE de bedst mulige rettigheder (BedDesigner/Editor, ikke
        // HaveArkitekt/Administrator) — så invitationens reelle loft (sat til ejerens
        // egne rettigheder, jf. CreateInvitationAsync når AllowSelfUpgrade er true)
        // bliver netop BedDesigner/Editor, og et forsøg på at acceptere med bedre
        // rettigheder end det skal afvises.
        var (_, rawToken) = await CreateInvitationAsync(
            testContext, sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
           GardenAccessLevel.BedDesigner, AccessCategory.Editor, allowSelfUpgrade: true,
           ownerLayer: GardenAccessLevel.BedDesigner, ownerCategory: AccessCategory.Editor);

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator,
            UpgradeBillingCycle: BillingCycle.Annual, AddOnQuantities: new Dictionary<Guid, int>());

        var act = () => sut.AcceptInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_AlreadyAccepted_ThrowsInvalidOperationException()
    {

        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var (_, rawToken) = await CreateInvitationAsync(
            CreateDbContextFactory(), sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            UpgradeBillingCycle: null, AddOnQuantities: new Dictionary<Guid, int>());
        await sut.AcceptInvitationAsync(request, TestContext.Current.CancellationToken);

        var act = () => sut.AcceptInvitationAsync(request with { UserId = "another-user" });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_RevokedInvitation_ThrowsInvalidOperationException()
    {

        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var (_, rawToken) = await CreateInvitationAsync(
            CreateDbContextFactory(), sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        var tokenService = new InvitationTokenService();
        using var context = CreateDbContext();
        {
            var invitation = await context.GardenInvitations.SingleAsync(i => i.TokenHash == tokenService.HashToken(rawToken), TestContext.Current.CancellationToken);
            invitation.IsRevoked = true;
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            UpgradeBillingCycle: null, AddOnQuantities: new Dictionary<Guid, int>());

        var act = () => sut.AcceptInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_ExpiredInvitation_ThrowsInvalidOperationException()
    {

        var timeProvider = new TestTimeProvider(FixedNow);
        var sut = CreateSut(CreateDbContextFactory(), timeProvider);

        var (_, rawToken) = await CreateInvitationAsync(
            CreateDbContextFactory(), sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        timeProvider.Advance(TimeSpan.FromDays(8));

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            UpgradeBillingCycle: null, AddOnQuantities: new Dictionary<Guid, int>());

        var act = () => sut.AcceptInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AcceptInvitationAsync_UserAlreadyMemberOfGarden_ThrowsInvalidOperationException()
    {

        var sut = CreateSut(CreateDbContextFactory(), new TestTimeProvider(FixedNow));

        var (gardenId, rawToken) = await CreateInvitationAsync(
            CreateDbContextFactory(), sut, GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer);

        using var context = CreateDbContext();
        {
            await context.GardenMemberships.AddAsync(new GardenMembership
            {
                GardenId = gardenId,
                UserId = "invited-user",
                IsOwner = false,
                Layer = GardenAccessLevel.Planlaegger,
                Category = AccessCategory.Viewer
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var request = new AcceptInvitationRequestDto(
            rawToken, "invited-user", GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            UpgradeBillingCycle: null, AddOnQuantities: new Dictionary<Guid, int>());

        var act = () => sut.AcceptInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
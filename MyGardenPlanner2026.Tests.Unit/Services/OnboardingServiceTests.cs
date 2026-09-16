namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using Xunit;

public sealed class OnboardingServiceTests : TestDbContext
{
    private OnboardingService CreateSut(TestTimeProvider timeProvider) =>
        new(CreateDbContextFactory(), new InvitationTokenService(), timeProvider, NullLogger<OnboardingService>.Instance);

    [Fact]
    public async Task CreateSandboxGardenAsync_CreatesOwnerMembershipAndTrialEntitlement()
    {
        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));

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
        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));

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
        var timeProvider = new TestTimeProvider(DateTimeOffset.Now);
        var sut = CreateSut(timeProvider);

        var request = new PaidGardenProvisionRequestDto(
            "user-1", "Min have", null, GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            BillingCycle.Annual, new Dictionary<Guid, int>());

        var result = await sut.ProvisionPaidGardenAsync(request, TestContext.Current.CancellationToken);

        await using var context = CreateDbContext();
        var entitlement = context.UserEntitlements.Single(e => e.Id == result.EntitlementId);

        entitlement.ValidToUtc.Should().Be(timeProvider.GetUtcNow().AddYears(1));
    }

    [Fact]
    public async Task ProvisionPaidGardenAsync_WithAddOnQuantities_IncrementsCorrectQuotaFields()
    {
        var addOn = new SubscriptionAddOn
        {
            Type = AddOnType.ArtefaktpakkeA,
            Name = "Artefaktpakke A",
            UnitDescription = "test",
            AnnualPrice = 1,
            MonthlyPrice = 1,
            PerpetualPrice = 1
        };
        await using (var context = CreateDbContext())
        {
            await context.SubscriptionAddOns.AddAsync(addOn, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));
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
        var garden = new Garden { Name = "Testhave" };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));
        var request = new CreateInvitationRequestDto(
            garden.Id, "not-a-member", "invited@example.com",
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            AllowSelfUpgrade: false, UseFreeSlot: false, ValidFor: TimeSpan.FromDays(7));

        var act = () => sut.CreateInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateInvitationAsync_TargetLayerBetterThanRequester_ThrowsInvalidOperationException()
    {
        var garden = new Garden { Name = "Testhave" };
        var requester = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "owner",
            IsOwner = true,
            Layer = GardenAccessLevel.Planlaegger,
            Category = AccessCategory.Editor
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(requester, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));
        var request = new CreateInvitationRequestDto(
            garden.Id, "owner", "invited@example.com",
            GardenAccessLevel.HaveArkitekt, AccessCategory.Editor,
            GardenAccessLevel.HaveArkitekt, AccessCategory.Editor,
            AllowSelfUpgrade: false, UseFreeSlot: false, ValidFor: TimeSpan.FromDays(7));

        var act = () => sut.CreateInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateInvitationAsync_ValidRequest_TokenHashMatchesGeneratedRawToken()
    {
        var garden = new Garden { Name = "Testhave" };
        var requester = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "owner",
            IsOwner = true,
            Layer = GardenAccessLevel.BedDesigner,
            Category = AccessCategory.Editor
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(requester, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var tokenService = new InvitationTokenService();
        var sut = new OnboardingService(CreateDbContextFactory(), tokenService, new TestTimeProvider(DateTimeOffset.Now), NullLogger<OnboardingService>.Instance);

        var request = new CreateInvitationRequestDto(
            garden.Id, "owner", "invited@example.com",
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            AllowSelfUpgrade: false, UseFreeSlot: false, ValidFor: TimeSpan.FromDays(7));

        var result = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        await using var verifyContext = CreateDbContext();
        var invitation = verifyContext.GardenInvitations.Single(i => i.Id == result.InvitationId);

        invitation.TokenHash.Should().Be(tokenService.HashToken(result.RawToken));
    }

    [Fact]
    public async Task CreateInvitationAsync_UseFreeSlotWithMismatchedLayer_ThrowsInvalidOperationException()
    {
        var garden = new Garden { Name = "Testhave" };
        var requester = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "owner",
            IsOwner = true,
            Layer = GardenAccessLevel.BedDesigner,
            Category = AccessCategory.Editor
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(requester, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "owner",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.BedDesigner,
                Category = AccessCategory.Editor,
                BillingCycle = BillingCycle.Annual,
                IsTrial = false
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));
        var request = new CreateInvitationRequestDto(
            garden.Id, "owner", "invited@example.com",
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            AllowSelfUpgrade: false, UseFreeSlot: true, ValidFor: TimeSpan.FromDays(7));

        var act = () => sut.CreateInvitationAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateInvitationAsync_UseFreeSlotWithAvailableQuota_SetsIsFreeSlotTrue()
    {
        var garden = new Garden { Name = "Testhave" };
        var requester = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "owner",
            IsOwner = true,
            Layer = GardenAccessLevel.BedDesigner,
            Category = AccessCategory.Editor
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(requester, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "owner",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.BedDesigner,
                Category = AccessCategory.Editor,
                BillingCycle = BillingCycle.Annual,
                IsTrial = false
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));
        var request = new CreateInvitationRequestDto(
            garden.Id, "owner", "invited@example.com",
            GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            GardenAccessLevel.BedDesigner, AccessCategory.Editor,
            AllowSelfUpgrade: false, UseFreeSlot: true, ValidFor: TimeSpan.FromDays(7));

        var result = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        result.IsFreeSlot.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateInvitationTokenAsync_ValidToken_ReturnsIsValidTrue()
    {
        var garden = new Garden { Name = "Testhave" };
        var requester = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = "owner",
            IsOwner = true,
            Layer = GardenAccessLevel.BedDesigner,
            Category = AccessCategory.Editor
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenMemberships.AddAsync(requester, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));
        var request = new CreateInvitationRequestDto(
            garden.Id, "owner", "invited@example.com",
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            GardenAccessLevel.Planlaegger, AccessCategory.Viewer,
            AllowSelfUpgrade: false, UseFreeSlot: false, ValidFor: TimeSpan.FromDays(7));
        var created = await sut.CreateInvitationAsync(request, TestContext.Current.CancellationToken);

        var result = await sut.ValidateInvitationTokenAsync(created.RawToken, TestContext.Current.CancellationToken);

        result.IsValid.Should().BeTrue();
        result.Invitation!.Email.Should().Be("invited@example.com");
    }

    [Fact]
    public async Task ValidateInvitationTokenAsync_UnknownToken_ReturnsIsValidFalse()
    {
        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));

        var result = await sut.ValidateInvitationTokenAsync("does-not-exist", TestContext.Current.CancellationToken);

        result.IsValid.Should().BeFalse();
        result.FailureReason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RevokeInvitationAsync_PendingInvitation_SetsIsRevokedTrue()
    {
        var garden = new Garden { Name = "Testhave" };
        var invitation = new GardenInvitation
        {
            GardenId = garden.Id,
            InvitedByUserId = "owner",
            Email = "invited@example.com",
            TokenHash = "hash",
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenInvitations.AddAsync(invitation, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));

        await sut.RevokeInvitationAsync(invitation.Id, "owner", TestContext.Current.CancellationToken);

        await using var verifyContext = CreateDbContext();
        verifyContext.GardenInvitations.Single(i => i.Id == invitation.Id).IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeInvitationAsync_AcceptedInvitation_ThrowsInvalidOperationException()
    {
        var garden = new Garden { Name = "Testhave" };
        var invitation = new GardenInvitation
        {
            GardenId = garden.Id,
            InvitedByUserId = "owner",
            Email = "invited@example.com",
            TokenHash = "hash",
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            IsAccepted = true
        };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.GardenInvitations.AddAsync(invitation, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));

        var act = () => sut.RevokeInvitationAsync(invitation.Id, "owner");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetFreeInvitationQuotaAsync_RevokedInvitationDoesNotCountAsUsed()
    {
        var garden = new Garden { Name = "Testhave" };
        await using (var context = CreateDbContext())
        {
            await context.Gardens.AddAsync(garden, TestContext.Current.CancellationToken);
            await context.UserEntitlements.AddAsync(new UserEntitlement
            {
                UserId = "owner",
                GardenId = garden.Id,
                Layer = GardenAccessLevel.BedDesigner,
                Category = AccessCategory.Editor,
                BillingCycle = BillingCycle.Annual,
                IsTrial = false
            }, TestContext.Current.CancellationToken);
            await context.GardenInvitations.AddAsync(new GardenInvitation
            {
                GardenId = garden.Id,
                InvitedByUserId = "owner",
                Email = "a@example.com",
                TokenHash = "hash",
                IsFreeSlot = true,
                IsRevoked = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            }, TestContext.Current.CancellationToken);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var sut = CreateSut(new TestTimeProvider(DateTimeOffset.Now));

        var quota = await sut.GetFreeInvitationQuotaAsync(garden.Id, "owner", TestContext.Current.CancellationToken);

        quota.TotalFreeSlots.Should().Be(1);
        quota.UsedFreeSlots.Should().Be(0);
        quota.RemainingFreeSlots.Should().Be(1);
    }
}
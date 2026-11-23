namespace MyGardenPlanner2026.Tests.Unit.Services.Onboarding;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using MyGardenPlanner2026.Infrastructure.Services.Onboarding;
using NSubstitute;
using System.Text.Json;
using Xunit;

public sealed class OnboardingServiceRestoreTests : IDisposable
{
    private const string FormerUserId = "user-old";
    private const string NewUserId = "user-new";
    private const string OwnerEmail = "bruger@example.dk";

    private static readonly DateTimeOffset Now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PlannerDbContext> _options;
    private readonly OnboardingService _sut;

    public OnboardingServiceRestoreTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite(_connection).Options;

        using (var context = new PlannerDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _currentUser.GetCurrent().Returns(new CurrentUserInfo(NewUserId, OwnerEmail, "10.0.0.1"));

        _sut = new OnboardingService(
            new TestFactory(_options), new InvitationTokenService(), new FixedTimeProvider(Now),
            NullLogger<OnboardingService>.Instance, _currentUser);
    }

    public void Dispose() => _connection.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class TestFactory(DbContextOptions<PlannerDbContext> options) : IDbContextFactory<PlannerDbContext>
    {
        public PlannerDbContext CreateDbContext() => new(options);
    }

    // ---------- Seed og opslag ----------

    /// <summary>Havens tidligere ejerniveau er som standard BedDesigner/Editor.</summary>
    private async Task<(Guid GardenId, Guid TicketId)> SeedDeletedGardenAsync(
        string name = "Slettet have",
        DateTimeOffset? expiresUtc = null,
        GardenAccessLevel formerLayer = GardenAccessLevel.BedDesigner,
        AccessCategory formerCategory = AccessCategory.Editor)
    {
        var deletedAt = Now.AddDays(-1);

        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = name, IsDeleted = true, DeletedAtUtc = deletedAt, DeletedByUserId = FormerUserId, DeletedByEmail = OwnerEmail };
        context.Gardens.Add(garden);
        context.GardenMemberships.Add(new GardenMembership
        {
            GardenId = garden.Id,
            UserId = FormerUserId,
            IsOwner = true,
            Layer = formerLayer,
            Category = formerCategory,
            IsDeleted = true,
            DeletedAtUtc = deletedAt,
            DeletedByUserId = FormerUserId,
            DeletedByEmail = OwnerEmail
        });
        context.UserEntitlements.Add(new UserEntitlement
        {
            GardenId = garden.Id,
            UserId = FormerUserId,
            Layer = formerLayer,
            Category = formerCategory,
            IsDeleted = true,
            DeletedAtUtc = deletedAt,
            DeletedByUserId = FormerUserId,
            DeletedByEmail = OwnerEmail
        });

        var ticket = new PendingGardenRestore
        {
            GardenId = garden.Id,
            OwnerEmailHash = RestoreEmailHasher.Hash(OwnerEmail),
            FormerUserId = FormerUserId,
            DeletedAtUtc = deletedAt,
            ExpiresUtc = expiresUtc ?? Now.AddDays(29)
        };
        context.PendingGardenRestores.Add(ticket);

        await context.SaveChangesAsync(Ct);
        return (garden.Id, ticket.Id);
    }

    private async Task<Guid> SeedDraftAsync(
        GardenAccessLevel layer, AccessCategory category, BillingCycle cycle = BillingCycle.Annual,
        DateTimeOffset? expiresUtc = null, Dictionary<Guid, int>? addOnQuantities = null)
    {
        await using var context = new PlannerDbContext(_options);
        var draft = new CheckoutDraft
        {
            GardenName = "Kladde",
            Layer = layer,
            Category = category,
            BillingCycle = cycle,
            ExpiresUtc = expiresUtc ?? Now.AddMinutes(10),
            AddOnQuantities = addOnQuantities ?? []
        };
        context.CheckoutDrafts.Add(draft);
        await context.SaveChangesAsync(Ct);
        return draft.Id;
    }

    private static PaidGardenRestoreRequestDto Request(
        Guid ticketId,
        GardenAccessLevel layer = GardenAccessLevel.BedDesigner,
        AccessCategory category = AccessCategory.Editor,
        BillingCycle cycle = BillingCycle.Annual,
        string userId = NewUserId,
        string email = OwnerEmail,
        IReadOnlyDictionary<Guid, int>? addOnQuantities = null) =>
        new(userId, email, ticketId, layer, category, cycle, addOnQuantities ?? new Dictionary<Guid, int>());

    private async Task<Garden?> FindGardenAsync(Guid gardenId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.Gardens.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == gardenId, Ct);
    }

    private async Task<GardenMembership?> FindMembershipAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.GardenMemberships.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.GardenId == gardenId && m.UserId == userId, Ct);
    }

    private async Task<UserEntitlement?> FindEntitlementAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.UserEntitlements.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.GardenId == gardenId && e.UserId == userId, Ct);
    }

    private async Task<List<PendingGardenRestore>> GetTicketsAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.PendingGardenRestores.ToListAsync(Ct);
    }

    private async Task<int> CountDraftsAsync(Guid draftId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.CheckoutDrafts.CountAsync(d => d.Id == draftId, Ct);
    }

    private async Task<Guid> SeedAddOnAsync(AddOnType type)
    {
        await using var context = new PlannerDbContext(_options);
        var addOn = new SubscriptionAddOn
        {
            Type = type,
            Name = type.ToString(),
            UnitDescription = "stk",
            AnnualPrice = 100m,
            MonthlyPrice = 10m,
            PerpetualPrice = 300m,
            DisplayOrder = (int)type
        };
        context.SubscriptionAddOns.Add(addOn);
        await context.SaveChangesAsync(Ct);
        return addOn.Id;
    }

    private async Task AssertNothingRestoredAsync(Guid gardenId)
    {
        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(gardenId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(gardenId, NewUserId)).Should().BeNull();
        (await FindEntitlementAsync(gardenId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await FindEntitlementAsync(gardenId, NewUserId)).Should().BeNull();
        (await GetTicketsAsync()).Should().ContainSingle();
    }

    // ---------- ProvisionRestoredGardenAsync ----------

    [Fact]
    public async Task ProvisionRestoredGardenAsync_Success_RestoresGardenAndGivesNewUserPurchasedAccess()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        var result = await _sut.ProvisionRestoredGardenAsync(
            Request(ticketId, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator), Ct);

        result.GardenId.Should().Be(gardenId);

        var garden = await FindGardenAsync(gardenId);
        garden!.IsDeleted.Should().BeFalse();
        garden.DeletedAtUtc.Should().BeNull();
        garden.DeletedByUserId.Should().BeNull();

        var membership = await FindMembershipAsync(gardenId, NewUserId);
        membership.Should().NotBeNull();
        membership!.Id.Should().Be(result.MembershipId);
        membership.IsOwner.Should().BeTrue();
        membership.IsDeleted.Should().BeFalse();
        membership.DeletedAtUtc.Should().BeNull();
        membership.DeletedByUserId.Should().BeNull();
        membership.Layer.Should().Be(GardenAccessLevel.HaveArkitekt);
        membership.Category.Should().Be(AccessCategory.Administrator);
        (await FindMembershipAsync(gardenId, FormerUserId)).Should().BeNull();

        (await FindEntitlementAsync(gardenId, FormerUserId)).Should().BeNull();
        var entitlement = await FindEntitlementAsync(gardenId, NewUserId);
        entitlement.Should().NotBeNull();
        entitlement!.Id.Should().Be(result.EntitlementId);
        entitlement.IsDeleted.Should().BeFalse();
        entitlement.IsTrial.Should().BeFalse();
        entitlement.Layer.Should().Be(GardenAccessLevel.HaveArkitekt);
        entitlement.Category.Should().Be(AccessCategory.Administrator);
        entitlement.BillingCycle.Should().Be(BillingCycle.Annual);
        entitlement.ValidToUtc.Should().Be(Now.AddYears(1));

        (await GetTicketsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_PerpetualPurchase_HasNoExpiry()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        await _sut.ProvisionRestoredGardenAsync(Request(ticketId, cycle: BillingCycle.Perpetual), Ct);

        (await FindEntitlementAsync(gardenId, NewUserId))!.ValidToUtc.Should().BeNull();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_WithAddOns_SetsQuotasOnNewEntitlement()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        var bedProposals = await SeedAddOnAsync(AddOnType.BedforslagNiveau2);
        var artifactsA = await SeedAddOnAsync(AddOnType.ArtefaktpakkeA);

        await _sut.ProvisionRestoredGardenAsync(
            Request(ticketId, addOnQuantities: new Dictionary<Guid, int> { [bedProposals] = 2, [artifactsA] = 3 }), Ct);

        var entitlement = await FindEntitlementAsync(gardenId, NewUserId);
        entitlement!.ExtraBedProposalsCount.Should().Be(2);
        entitlement.ExtraCategoryAArtifactsCount.Should().Be(3);
        entitlement.ExtraPlannedBedsCount.Should().Be(0);
        entitlement.ExtraCategoryBArtifactsCount.Should().Be(0);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_UnknownAddOn_ThrowsAndRollsBackEverything()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        var act = () => _sut.ProvisionRestoredGardenAsync(
            Request(ticketId, addOnQuantities: new Dictionary<Guid, int> { [Guid.NewGuid()] = 1 }), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Tilkøb*findes ikke*");
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenFromDraftAsync_WithAddOns_UsesDraftQuotasAndConsumesDraft()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        var plannedBeds = await SeedAddOnAsync(AddOnType.PlanlagteBedeNiveau3);
        var draftId = await SeedDraftAsync(
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, addOnQuantities: new Dictionary<Guid, int> { [plannedBeds] = 4 });

        await _sut.ProvisionRestoredGardenFromDraftAsync(draftId, NewUserId, OwnerEmail, ticketId, Ct);

        (await FindEntitlementAsync(gardenId, NewUserId))!.ExtraPlannedBedsCount.Should().Be(4);
        (await CountDraftsAsync(draftId)).Should().Be(0);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_Success_RestoredRowsAreVisibleThroughQueryFilters()
    {
        var (_, ticketId) = await SeedDeletedGardenAsync();

        await _sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        await using var context = new PlannerDbContext(_options);
        (await context.Gardens.CountAsync(Ct)).Should().Be(1);
        (await context.GardenMemberships.CountAsync(m => m.UserId == NewUserId, Ct)).Should().Be(1);
        (await context.UserEntitlements.CountAsync(e => e.UserId == NewUserId, Ct)).Should().Be(1);
    }

    [Theory]
    [InlineData(GardenAccessLevel.BedDesigner, AccessCategory.Editor)]
    [InlineData(GardenAccessLevel.BedDesigner, AccessCategory.Administrator)]
    [InlineData(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor)]
    [InlineData(GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator)]
    public async Task ProvisionRestoredGardenAsync_PurchaseCoveringFormerLevel_Succeeds(
        GardenAccessLevel layer, AccessCategory category)
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        await _sut.ProvisionRestoredGardenAsync(Request(ticketId, layer, category), Ct);

        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeFalse();
    }

    [Theory]
    [InlineData(GardenAccessLevel.Planlaegger, AccessCategory.Editor)]
    [InlineData(GardenAccessLevel.BedDesigner, AccessCategory.ViewerPlus)]
    [InlineData(GardenAccessLevel.Planlaegger, AccessCategory.Viewer)]
    public async Task ProvisionRestoredGardenAsync_PurchaseNotCoveringFormerLevel_ThrowsAndChangesNothing(
        GardenAccessLevel layer, AccessCategory category)
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        var act = () => _sut.ProvisionRestoredGardenAsync(Request(ticketId, layer, category), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*dækker*");
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_OtherEmail_ThrowsAndChangesNothing()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        var act = () => _sut.ProvisionRestoredGardenAsync(Request(ticketId, email: "anden@example.dk"), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_EmailIsCaseAndWhitespaceInsensitive()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        var result = await _sut.ProvisionRestoredGardenAsync(Request(ticketId, email: " BRUGER@example.dk "), Ct);

        result.GardenId.Should().Be(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_UnknownTicket_Throws()
    {
        await SeedDeletedGardenAsync();

        var act = () => _sut.ProvisionRestoredGardenAsync(Request(Guid.NewGuid()), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_Expired_ThrowsAndChangesNothing()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync(expiresUtc: Now.AddSeconds(-1));

        var act = () => _sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*udløbet*");
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_ExpiringExactlyNow_Succeeds()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync(expiresUtc: Now);

        await _sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_SecondCallForSameTicket_Throws()
    {
        var (_, ticketId) = await SeedDeletedGardenAsync();
        await _sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        var act = () => _sut.ProvisionRestoredGardenAsync(Request(ticketId, userId: "user-third"), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_OwnerMembershipMissing_ThrowsAndChangesNothing()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        await using (var context = new PlannerDbContext(_options))
        {
            await context.GardenMemberships.IgnoreQueryFilters()
                .Where(m => m.GardenId == gardenId)
                .ExecuteDeleteAsync(Ct);
        }

        var act = () => _sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeTrue();
        (await GetTicketsAsync()).Should().ContainSingle();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_OnlyRestoresTheChosenGarden()
    {
        var (firstId, firstTicket) = await SeedDeletedGardenAsync("A");
        var (secondId, secondTicket) = await SeedDeletedGardenAsync("B");

        await _sut.ProvisionRestoredGardenAsync(Request(firstTicket), Ct);

        (await FindGardenAsync(firstId))!.IsDeleted.Should().BeFalse();
        (await FindGardenAsync(secondId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(secondId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await FindEntitlementAsync(secondId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await GetTicketsAsync()).Should().ContainSingle().Which.Id.Should().Be(secondTicket);
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_NullRequest_Throws()
    {
        var act = () => _sut.ProvisionRestoredGardenAsync(null!, Ct);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", OwnerEmail)]
    [InlineData("  ", OwnerEmail)]
    [InlineData(NewUserId, "")]
    [InlineData(NewUserId, "  ")]
    public async Task ProvisionRestoredGardenAsync_BlankUserOrEmail_Throws(string userId, string email)
    {
        var act = () => _sut.ProvisionRestoredGardenAsync(Request(Guid.NewGuid(), userId: userId, email: email), Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------- ProvisionRestoredGardenFromDraftAsync ----------

    [Fact]
    public async Task ProvisionRestoredGardenFromDraftAsync_Success_UsesDraftPurchaseAndConsumesDraft()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        var draftId = await SeedDraftAsync(GardenAccessLevel.HaveArkitekt, AccessCategory.Editor, BillingCycle.Monthly);

        var result = await _sut.ProvisionRestoredGardenFromDraftAsync(draftId, NewUserId, OwnerEmail, ticketId, Ct);

        result.GardenId.Should().Be(gardenId);
        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeFalse();

        var membership = await FindMembershipAsync(gardenId, NewUserId);
        membership!.Layer.Should().Be(GardenAccessLevel.HaveArkitekt);
        membership.Category.Should().Be(AccessCategory.Editor);

        var entitlement = await FindEntitlementAsync(gardenId, NewUserId);
        entitlement!.BillingCycle.Should().Be(BillingCycle.Monthly);
        entitlement.ValidToUtc.Should().Be(Now.AddMonths(1));

        (await CountDraftsAsync(draftId)).Should().Be(0);
        (await GetTicketsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ProvisionRestoredGardenFromDraftAsync_UnknownDraft_Throws()
    {
        var (_, ticketId) = await SeedDeletedGardenAsync();

        var act = () => _sut.ProvisionRestoredGardenFromDraftAsync(Guid.NewGuid(), NewUserId, OwnerEmail, ticketId, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ProvisionRestoredGardenFromDraftAsync_ExpiredDraft_RemovesDraftThrowsAndChangesNothing()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        var draftId = await SeedDraftAsync(
            GardenAccessLevel.BedDesigner, AccessCategory.Editor, expiresUtc: Now.AddSeconds(-1));

        var act = () => _sut.ProvisionRestoredGardenFromDraftAsync(draftId, NewUserId, OwnerEmail, ticketId, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*udløbet*");
        (await CountDraftsAsync(draftId)).Should().Be(0);
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenFromDraftAsync_PurchaseNotCovering_ThrowsKeepsDraftAndChangesNothing()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        var draftId = await SeedDraftAsync(GardenAccessLevel.Planlaegger, AccessCategory.Editor);

        var act = () => _sut.ProvisionRestoredGardenFromDraftAsync(draftId, NewUserId, OwnerEmail, ticketId, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*dækker*");
        (await CountDraftsAsync(draftId)).Should().Be(1);
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task ProvisionRestoredGardenFromDraftAsync_OtherEmail_ThrowsKeepsDraftAndChangesNothing()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();
        var draftId = await SeedDraftAsync(GardenAccessLevel.BedDesigner, AccessCategory.Editor);

        var act = () => _sut.ProvisionRestoredGardenFromDraftAsync(draftId, NewUserId, "anden@example.dk", ticketId, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await CountDraftsAsync(draftId)).Should().Be(1);
        await AssertNothingRestoredAsync(gardenId);
    }

    // ---------- Rundtur: kontosletning -> gendannelse ved køb ----------

    [Fact]
    public async Task DeleteAccountThenRestoreWithPurchase_RoundTrip_GivesNewUserTheGarden()
    {
        Guid gardenId;
        await using (var context = new PlannerDbContext(_options))
        {
            var garden = new Garden { Name = "Rundtur" };
            gardenId = garden.Id;
            context.Gardens.Add(garden);
            context.GardenMemberships.Add(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = FormerUserId,
                IsOwner = true,
                Layer = GardenAccessLevel.Planlaegger,
                Category = AccessCategory.Administrator
            });
            context.UserEntitlements.Add(new UserEntitlement { GardenId = garden.Id, UserId = FormerUserId });
            await context.SaveChangesAsync(Ct);
        }

        var restoreOptions = Substitute.For<IOptionsMonitor<GardenRestoreOptions>>();
        restoreOptions.CurrentValue.Returns(new GardenRestoreOptions { RetentionDays = 30 });
        var deletion = new AccountDeletionService(
            new TestFactory(_options), Substitute.For<IReAuthFailureTracker>(), Substitute.For<IAdminSoftDeleteActorScrubber>(), _currentUser, restoreOptions,
            new FixedTimeProvider(Now), NullLogger<AccountDeletionService>.Instance);
        var restoreService = new GardenRestoreService(new TestFactory(_options), new FixedTimeProvider(Now));

        await deletion.DeleteUserDataAsync(FormerUserId, OwnerEmail, Ct);

        var restorable = (await restoreService.GetRestorableGardensAsync(OwnerEmail, Ct)).Should().ContainSingle().Subject;
        restorable.GardenId.Should().Be(gardenId);
        restorable.Layer.Should().Be(GardenAccessLevel.Planlaegger);
        restorable.Category.Should().Be(AccessCategory.Administrator);

        await _sut.ProvisionRestoredGardenAsync(
            Request(restorable.PendingRestoreId, GardenAccessLevel.Planlaegger, AccessCategory.Administrator), Ct);

        await using var verify = new PlannerDbContext(_options);
        (await verify.Gardens.CountAsync(Ct)).Should().Be(1);
        (await verify.GardenMemberships.SingleAsync(Ct)).UserId.Should().Be(NewUserId);
        (await verify.UserEntitlements.CountAsync(e => e.UserId == NewUserId, Ct)).Should().Be(1);
        (await restoreService.GetRestorableGardensAsync(OwnerEmail, Ct)).Should().BeEmpty();
    }

    private async Task<List<AuditLog>> GetAuditAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.AuditLogs.AsNoTracking().OrderBy(a => a.Id).ToListAsync(Ct);
    }

    private static string Prop(string? json, string name) =>
        JsonElement.Parse(json!).GetProperty(name).ToString();

    // ---------- R10.4: audit ----------

    [Fact]
    public async Task ProvisionRestoredGardenAsync_Success_WritesGardenAndMembershipAuditRows()
    {
        var (gardenId, ticketId) = await SeedDeletedGardenAsync();

        var result = await _sut.ProvisionRestoredGardenAsync(
            Request(ticketId, GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator), Ct);

        var audit = await GetAuditAsync();

        var gardenRow = audit.Single(a => a.EntityName == "Garden");
        gardenRow.Action.Should().Be(AuditAction.Update);
        gardenRow.EntityId.Should().Be(gardenId.ToString());
        gardenRow.UserId.Should().Be(NewUserId);
        gardenRow.UserEmail.Should().Be(OwnerEmail);
        gardenRow.IpAddress.Should().Be("10.0.0.1");
        gardenRow.TimestampUtc.Should().Be(Now);
        Prop(gardenRow.OldValues, "IsDeleted").Should().Be("True");
        Prop(gardenRow.NewValues, "IsDeleted").Should().Be("False");
        Prop(gardenRow.NewValues, "RestoredByUserId").Should().Be(NewUserId);

        var membershipRow = audit.Single(a => a.EntityName == "GardenMembership");
        membershipRow.Action.Should().Be(AuditAction.Update);
        membershipRow.EntityId.Should().Be(result.MembershipId.ToString());
        Prop(membershipRow.OldValues, "UserId").Should().Be(FormerUserId);
        Prop(membershipRow.NewValues, "UserId").Should().Be(NewUserId);
        Prop(membershipRow.NewValues, "Layer").Should().Be("HaveArkitekt");
        Prop(membershipRow.NewValues, "Category").Should().Be("Administrator");
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_Rejected_WritesNoAuditRows()
    {
        var (_, ticketId) = await SeedDeletedGardenAsync();

        var act = () => _sut.ProvisionRestoredGardenAsync(
            Request(ticketId, GardenAccessLevel.Planlaegger, AccessCategory.Editor), Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await GetAuditAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_AccessorWithoutUser_UsesPurchasingUserAndKeepsIp()
    {
        _currentUser.GetCurrent().Returns(new CurrentUserInfo(null, null, "10.0.0.2"));
        var (_, ticketId) = await SeedDeletedGardenAsync();

        await _sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        var gardenRow = (await GetAuditAsync()).Single(a => a.EntityName == "Garden");
        gardenRow.UserId.Should().Be(NewUserId);
        gardenRow.UserEmail.Should().Be(OwnerEmail);
        gardenRow.IpAddress.Should().Be("10.0.0.2");
    }

    [Fact]
    public async Task ProvisionRestoredGardenAsync_WithoutAccessor_UsesPurchasingUserWithoutIp()
    {
        var sut = new OnboardingService(
            new TestFactory(_options), new InvitationTokenService(), new FixedTimeProvider(Now),
            NullLogger<OnboardingService>.Instance);
        var (_, ticketId) = await SeedDeletedGardenAsync();

        await sut.ProvisionRestoredGardenAsync(Request(ticketId), Ct);

        var gardenRow = (await GetAuditAsync()).Single(a => a.EntityName == "Garden");
        gardenRow.UserId.Should().Be(NewUserId);
        gardenRow.IpAddress.Should().BeNull();
    }
}
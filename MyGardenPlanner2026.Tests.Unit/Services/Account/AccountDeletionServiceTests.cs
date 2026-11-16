namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using NSubstitute;
using Xunit;

public sealed class AccountDeletionServiceTests : IDisposable
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";
    private const string ThirdUserId = "user-3";
    private const string FourthUserId = "user-4";
    private const string UserEmail = "bruger@example.dk";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PlannerDbContext> _options;
    private readonly IReAuthFailureTracker _tracker = Substitute.For<IReAuthFailureTracker>();
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly AccountDeletionService _sut;

    public AccountDeletionServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite(_connection).Options;

        using (var context = new PlannerDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _sut = CreateSut(retentionDays: 30);
    }

    private AccountDeletionService CreateSut(int retentionDays)
    {
        var restoreOptions = Substitute.For<IOptionsMonitor<GardenRestoreOptions>>();
        restoreOptions.CurrentValue.Returns(new GardenRestoreOptions { RetentionDays = retentionDays });

        return new AccountDeletionService(
            new TestFactory(_options), _tracker, restoreOptions, new FixedTimeProvider(Now),
            NullLogger<AccountDeletionService>.Instance);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    public void Dispose() => _connection.Dispose();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class TestFactory(DbContextOptions<PlannerDbContext> options) : IDbContextFactory<PlannerDbContext>
    {
        public PlannerDbContext CreateDbContext() => new(options);
    }

    // ---------- Seed ----------

    private async Task<Garden> SeedGardenAsync(string name, params (string UserId, bool IsOwner)[] members)
    {
        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = name };
        context.Gardens.Add(garden);
        foreach (var (userId, isOwner) in members)
        {
            context.GardenMemberships.Add(new GardenMembership { GardenId = garden.Id, UserId = userId, IsOwner = isOwner });
        }

        await context.SaveChangesAsync(Ct);
        return garden;
    }

    private async Task SeedEntitlementAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        context.UserEntitlements.Add(new UserEntitlement { GardenId = gardenId, UserId = userId });
        await context.SaveChangesAsync(Ct);
    }

    private async Task SeedDraftAsync(string userId)
    {
        await using var context = new PlannerDbContext(_options);
        context.CheckoutDrafts.Add(new CheckoutDraft
        {
            UserId = userId,
            GardenName = "Kladde",
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync(Ct);
    }

    private async Task<Guid> SeedInvitationAsync(
        Guid gardenId, string invitedBy, bool accepted = false, bool revoked = false,
        string email = "inviteret@example.dk")
    {
        await using var context = new PlannerDbContext(_options);
        var invitation = new GardenInvitation
        {
            GardenId = gardenId,
            InvitedByUserId = invitedBy,
            Email = email,
            TokenHash = Guid.NewGuid().ToString("N"),
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
            IsAccepted = accepted,
            IsRevoked = revoked
        };
        context.GardenInvitations.Add(invitation);
        await context.SaveChangesAsync(Ct);
        return invitation.Id;
    }

    private async Task SeedSoftDeletedMembershipAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        context.GardenMemberships.Add(new GardenMembership
        {
            GardenId = gardenId,
            UserId = userId,
            IsDeleted = true,
            DeletedAtUtc = DateTimeOffset.UtcNow
        });
        await context.SaveChangesAsync(Ct);
    }

    private async Task<Garden> SeedGardenWithRolesAsync(
    string name,
    params (string UserId, bool IsOwner, GardenAccessLevel Layer, AccessCategory Category)[] members)
    {
        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = name };
        context.Gardens.Add(garden);
        foreach (var (userId, isOwner, layer, category) in members)
        {
            context.GardenMemberships.Add(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = userId,
                IsOwner = isOwner,
                Layer = layer,
                Category = category
            });
        }

        await context.SaveChangesAsync(Ct);
        return garden;
    }

    private async Task<GardenMembership?> FindMembershipAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.GardenMemberships.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.GardenId == gardenId && m.UserId == userId, Ct);
    }

    private static Dictionary<Guid, GardenOwnershipResolution> Transfer(Guid gardenId, string newOwnerUserId) =>
        new() { [gardenId] = GardenOwnershipResolution.TransferTo(newOwnerUserId) };

    private static Dictionary<Guid, GardenOwnershipResolution> DeleteForAll(Guid gardenId) =>
        new() { [gardenId] = GardenOwnershipResolution.DeleteForAll() };

    private async Task<Garden?> FindGardenAsync(Guid gardenId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.Gardens.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == gardenId, Ct);
    }

    private async Task<UserEntitlement?> FindEntitlementAsync(Guid gardenId, string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.UserEntitlements.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.GardenId == gardenId && e.UserId == userId, Ct);
    }

    private async Task<List<PendingGardenRestore>> GetPendingAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.PendingGardenRestores.ToListAsync(Ct);
    }

    // ---------- Opslag (uden query-filtre) ----------

    private async Task<int> CountGardensAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.Gardens.IgnoreQueryFilters().CountAsync(Ct);
    }

    private async Task<int> CountMembershipsAsync(string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.GardenMemberships.IgnoreQueryFilters().CountAsync(m => m.UserId == userId, Ct);
    }

    private async Task<int> CountEntitlementsAsync(string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.UserEntitlements.IgnoreQueryFilters().CountAsync(e => e.UserId == userId, Ct);
    }

    private async Task<int> CountDraftsAsync(string userId)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.CheckoutDrafts.CountAsync(d => d.UserId == userId, Ct);
    }

    private async Task<GardenInvitation?> FindInvitationAsync(Guid id)
    {
        await using var context = new PlannerDbContext(_options);
        return await context.GardenInvitations.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.Id == id, Ct);
    }

    // ---------- Tests ----------

    [Fact]
    public async Task DeleteUserDataAsync_BlankUserId_Throws()
    {
        var act = () => _sut.DeleteUserDataAsync(" ", UserEmail, Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteUserDataAsync_SoleMember_RetainsGardenSoftDeletedAndCreatesPendingRestore()
    {
        var garden = await SeedGardenAsync("Min have", (UserId, true));
        await SeedEntitlementAsync(garden.Id, UserId);
        await SeedDraftAsync(UserId);

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeFalse();
        var deletedGarden = await FindGardenAsync(garden.Id);
        deletedGarden!.IsDeleted.Should().BeTrue();
        deletedGarden.DeletedBy.Should().Be(UserId);
        deletedGarden.DeletedAtUtc.Should().Be(Now);
        (await FindMembershipAsync(garden.Id, UserId))!.IsDeleted.Should().BeTrue();
        (await FindEntitlementAsync(garden.Id, UserId))!.IsDeleted.Should().BeTrue();
        (await CountDraftsAsync(UserId)).Should().Be(0);

        var pending = (await GetPendingAsync()).Should().ContainSingle().Subject;
        pending.GardenId.Should().Be(garden.Id);
        pending.OwnerEmailHash.Should().Be(RestoreEmailHasher.Hash(UserEmail));
        pending.FormerUserId.Should().Be(UserId);
        pending.DeletedAtUtc.Should().Be(Now);
        pending.ExpiresUtc.Should().Be(Now.AddDays(30));
    }

    [Fact]
    public async Task DeleteUserDataAsync_SharedGardenAsNonOwner_RemovesOnlyOwnMembership()
    {
        var garden = await SeedGardenAsync("Fælles have", (OtherUserId, true), (UserId, false));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeFalse();
        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync(UserId)).Should().Be(0);
        (await CountMembershipsAsync(OtherUserId)).Should().Be(1);
        garden.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteUserDataAsync_OwnerWithAnotherOwner_RemovesOnlyOwnMembership()
    {
        await SeedGardenAsync("Fælles have", (UserId, true), (OtherUserId, true));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeFalse();
        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync(UserId)).Should().Be(0);
        (await CountMembershipsAsync(OtherUserId)).Should().Be(1);
    }

    [Fact]
    public async Task DeleteUserDataAsync_SoleOwnerWithOtherMembers_IsBlockedAndChangesNothing()
    {
        var garden = await SeedGardenAsync("Blokeret have", (UserId, true), (OtherUserId, false));
        await SeedEntitlementAsync(garden.Id, UserId);
        await SeedDraftAsync(UserId);

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeTrue();
        result.BlockingGardenNames.Should().Equal("Blokeret have");
        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync(UserId)).Should().Be(1);
        (await CountEntitlementsAsync(UserId)).Should().Be(1);
        (await CountDraftsAsync(UserId)).Should().Be(1);
        await _tracker.DidNotReceive().ClearFailuresAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserDataAsync_BlockedGarden_PreventsDeletionOfOtherGardens()
    {
        await SeedGardenAsync("Blokeret have", (UserId, true), (OtherUserId, false));
        await SeedGardenAsync("Egen have", (UserId, true));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeTrue();
        (await CountGardensAsync()).Should().Be(2);
    }

    [Fact]
    public async Task DeleteUserDataAsync_SoftDeletedRows_AreAlsoRemoved()
    {
        var garden = await SeedGardenAsync("Fælles have", (OtherUserId, true));
        await SeedSoftDeletedMembershipAsync(garden.Id, UserId);

        await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        (await CountMembershipsAsync(UserId)).Should().Be(0);
        (await CountMembershipsAsync(OtherUserId)).Should().Be(1);
    }

    [Fact]
    public async Task DeleteUserDataAsync_InvitationsSentByUser_AcceptedAndRevokedDeletedPendingRevoked()
    {
        var garden = await SeedGardenAsync("Fælles have", (OtherUserId, true), (UserId, false));
        var accepted = await SeedInvitationAsync(garden.Id, UserId, accepted: true);
        var revoked = await SeedInvitationAsync(garden.Id, UserId, revoked: true);
        var pending = await SeedInvitationAsync(garden.Id, UserId);
        var othersPending = await SeedInvitationAsync(garden.Id, OtherUserId);

        await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        (await FindInvitationAsync(accepted)).Should().BeNull();
        (await FindInvitationAsync(revoked)).Should().BeNull();
        (await FindInvitationAsync(pending))!.IsRevoked.Should().BeTrue();
        (await FindInvitationAsync(othersPending))!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserDataAsync_InvitationsToUsersEmail_AreDeletedRegardlessOfStateAndCase()
    {
        var garden = await SeedGardenAsync("Fælles have", (OtherUserId, true), (UserId, false));
        var pending = await SeedInvitationAsync(garden.Id, OtherUserId, email: "Bruger@Example.DK");
        var accepted = await SeedInvitationAsync(garden.Id, OtherUserId, accepted: true, email: UserEmail);
        var revoked = await SeedInvitationAsync(garden.Id, OtherUserId, revoked: true, email: UserEmail);
        var toSomeoneElse = await SeedInvitationAsync(garden.Id, OtherUserId, email: "anden@example.dk");

        await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        (await FindInvitationAsync(pending)).Should().BeNull();
        (await FindInvitationAsync(accepted)).Should().BeNull();
        (await FindInvitationAsync(revoked)).Should().BeNull();
        (await FindInvitationAsync(toSomeoneElse)).Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteUserDataAsync_NoEmail_SkipsEmailInvitations()
    {
        var garden = await SeedGardenAsync("Fælles have", (OtherUserId, true), (UserId, false));
        var toUsersEmail = await SeedInvitationAsync(garden.Id, OtherUserId, email: UserEmail);

        var result = await _sut.DeleteUserDataAsync(UserId, null, Ct);

        result.IsBlocked.Should().BeFalse();
        (await FindInvitationAsync(toUsersEmail)).Should().NotBeNull();
    }
    [Fact]
    public async Task DeleteUserDataAsync_OtherUsersData_IsUntouched()
    {
        var garden = await SeedGardenAsync("Anden have", (OtherUserId, true));
        await SeedEntitlementAsync(garden.Id, OtherUserId);
        await SeedDraftAsync(OtherUserId);
        await SeedDraftAsync(UserId);

        await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync(OtherUserId)).Should().Be(1);
        (await CountEntitlementsAsync(OtherUserId)).Should().Be(1);
        (await CountDraftsAsync(OtherUserId)).Should().Be(1);
        (await CountDraftsAsync(UserId)).Should().Be(0);
    }

    [Fact]
    public async Task DeleteUserDataAsync_Success_ClearsReAuthFailures()
    {
        await SeedGardenAsync("Min have", (UserId, true));

        await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        await _tracker.Received(1).ClearFailuresAsync(UserId, Arg.Any<CancellationToken>());
    }

    // ---------- R10.1a: ejerskabsoverdragelse ----------

    [Fact]
    public async Task GetBlockingGardensAsync_SoleOwnerWithOtherMembers_ReturnsCandidatesOrderedByCategoryDescending()
    {
        var garden = await SeedGardenWithRolesAsync("Blokeret have",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.BedDesigner, AccessCategory.Editor),
            (ThirdUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (FourthUserId, false, GardenAccessLevel.HaveArkitekt, AccessCategory.Viewer));

        var result = await _sut.GetBlockingGardensAsync(UserId, Ct);

        var blocking = result.Should().ContainSingle().Subject;
        blocking.GardenId.Should().Be(garden.Id);
        blocking.Name.Should().Be("Blokeret have");
        blocking.Candidates.Select(c => c.UserId).Should().Equal(ThirdUserId, OtherUserId, FourthUserId);
    }

    [Fact]
    public async Task GetBlockingGardensAsync_NoBlockingGardens_ReturnsEmpty()
    {
        await SeedGardenAsync("Egen have", (UserId, true));
        await SeedGardenAsync("Delt ejerskab", (UserId, true), (OtherUserId, true));
        await SeedGardenAsync("Som medlem", (OtherUserId, true), (UserId, false));

        var result = await _sut.GetBlockingGardensAsync(UserId, Ct);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBlockingGardensAsync_ChangesNothing()
    {
        await SeedGardenAsync("Blokeret have", (UserId, true), (OtherUserId, false));

        await _sut.GetBlockingGardensAsync(UserId, Ct);

        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync(UserId)).Should().Be(1);
        (await CountMembershipsAsync(OtherUserId)).Should().Be(1);
    }

    [Fact]
    public async Task GetBlockingGardensAsync_BlankUserId_Throws()
    {
        var act = () => _sut.GetBlockingGardensAsync(" ", Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteUserDataAsync_NullResolutions_Throws()
    {
        var act = () => _sut.DeleteUserDataAsync(UserId, UserEmail, null!, Ct);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteUserDataAsync_TransferToEditor_PromotesToAdministratorKeepsLayerAndDeletesOwnMembership()
    {
        var garden = await SeedGardenWithRolesAsync("Overdrages",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.BedDesigner, AccessCategory.Editor));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(garden.Id, OtherUserId), Ct);

        result.IsBlocked.Should().BeFalse();
        (await CountGardensAsync()).Should().Be(1);
        (await FindMembershipAsync(garden.Id, UserId)).Should().BeNull();
        var newOwner = await FindMembershipAsync(garden.Id, OtherUserId);
        newOwner.Should().NotBeNull();
        newOwner!.IsOwner.Should().BeTrue();
        newOwner.Category.Should().Be(AccessCategory.Administrator);
        newOwner.Layer.Should().Be(GardenAccessLevel.BedDesigner);
    }

    [Fact]
    public async Task DeleteUserDataAsync_TransferToAdministrator_StaysAdministrator()
    {
        var garden = await SeedGardenWithRolesAsync("Overdrages",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Administrator));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(garden.Id, OtherUserId), Ct);

        result.IsBlocked.Should().BeFalse();
        var newOwner = await FindMembershipAsync(garden.Id, OtherUserId);
        newOwner!.IsOwner.Should().BeTrue();
        newOwner.Category.Should().Be(AccessCategory.Administrator);
    }

    [Fact]
    public async Task DeleteUserDataAsync_Transfer_DeletesUsersEntitlementButKeepsNewOwnersEntitlement()
    {
        var garden = await SeedGardenWithRolesAsync("Overdrages",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));
        await SeedEntitlementAsync(garden.Id, UserId);
        await SeedEntitlementAsync(garden.Id, OtherUserId);

        await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(garden.Id, OtherUserId), Ct);

        (await CountEntitlementsAsync(UserId)).Should().Be(0);
        (await CountEntitlementsAsync(OtherUserId)).Should().Be(1);
    }

    [Theory]
    [InlineData("ukendt-bruger")]
    [InlineData(UserId)]
    public async Task DeleteUserDataAsync_InvalidRecipient_IsBlockedAndChangesNothing(string recipient)
    {
        var garden = await SeedGardenWithRolesAsync("Blokeret have",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(garden.Id, recipient), Ct);

        result.IsBlocked.Should().BeTrue();
        result.BlockingGardenNames.Should().Equal("Blokeret have");
        (await CountMembershipsAsync(UserId)).Should().Be(1);
        var other = await FindMembershipAsync(garden.Id, OtherUserId);
        other!.IsOwner.Should().BeFalse();
        other.Category.Should().Be(AccessCategory.Editor);
        await _tracker.DidNotReceive().ClearFailuresAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteUserDataAsync_SoftDeletedRecipient_IsBlocked()
    {
        var garden = await SeedGardenWithRolesAsync("Blokeret have",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));
        await SeedSoftDeletedMembershipAsync(garden.Id, ThirdUserId);

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(garden.Id, ThirdUserId), Ct);

        result.IsBlocked.Should().BeTrue();
        (await CountMembershipsAsync(UserId)).Should().Be(1);
    }

    [Fact]
    public async Task DeleteUserDataAsync_OnlyOneOfTwoBlockingGardensResolved_IsBlockedWithUnresolvedAndChangesNothing()
    {
        var resolved = await SeedGardenWithRolesAsync("A - løst",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));
        await SeedGardenWithRolesAsync("B - uløst",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(resolved.Id, OtherUserId), Ct);

        result.IsBlocked.Should().BeTrue();
        result.BlockingGardenNames.Should().Equal("B - uløst");
        (await CountGardensAsync()).Should().Be(2);
        (await CountMembershipsAsync(UserId)).Should().Be(2);
        var other = await FindMembershipAsync(resolved.Id, OtherUserId);
        other!.IsOwner.Should().BeFalse();
        other.Category.Should().Be(AccessCategory.Editor);
    }

    [Fact]
    public async Task DeleteUserDataAsync_ResolutionForNonBlockingGarden_IsIgnored()
    {
        var garden = await SeedGardenWithRolesAsync("Delt ejerskab",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Editor));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Transfer(garden.Id, ThirdUserId), Ct);

        result.IsBlocked.Should().BeFalse();
        (await FindMembershipAsync(garden.Id, UserId)).Should().BeNull();
        var other = await FindMembershipAsync(garden.Id, OtherUserId);
        other!.Category.Should().Be(AccessCategory.Editor);
    }

    // ---------- R10.1b: slet for alle + opbevaring ----------

    [Fact]
    public async Task DeleteUserDataAsync_SoleMemberWithoutEmail_DeletesGardenPhysicallyAndCreatesNoPendingRestore()
    {
        await SeedGardenAsync("Min have", (UserId, true));

        var result = await _sut.DeleteUserDataAsync(UserId, null, Ct);

        result.IsBlocked.Should().BeFalse();
        (await CountGardensAsync()).Should().Be(0);
        (await CountMembershipsAsync(UserId)).Should().Be(0);
        (await GetPendingAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteUserDataAsync_UsesConfiguredRetentionDays()
    {
        var sut = CreateSut(retentionDays: 7);
        await SeedGardenAsync("Min have", (UserId, true));

        await sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        (await GetPendingAsync()).Should().ContainSingle().Which.ExpiresUtc.Should().Be(Now.AddDays(7));
    }

    [Fact]
    public async Task DeleteUserDataAsync_DeleteForAll_RetainsOwnersRowsAndRemovesOtherMembersAccess()
    {
        var garden = await SeedGardenWithRolesAsync("Slettes for alle",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor),
            (ThirdUserId, false, GardenAccessLevel.HaveArkitekt, AccessCategory.Viewer));
        await SeedEntitlementAsync(garden.Id, UserId);
        await SeedEntitlementAsync(garden.Id, OtherUserId);
        var invitation = await SeedInvitationAsync(garden.Id, OtherUserId);

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, DeleteForAll(garden.Id), Ct);

        result.IsBlocked.Should().BeFalse();
        (await FindGardenAsync(garden.Id))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(garden.Id, UserId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(garden.Id, OtherUserId)).Should().BeNull();
        (await FindMembershipAsync(garden.Id, ThirdUserId)).Should().BeNull();
        (await FindEntitlementAsync(garden.Id, UserId))!.IsDeleted.Should().BeTrue();
        (await FindEntitlementAsync(garden.Id, OtherUserId)).Should().BeNull();
        (await FindInvitationAsync(invitation)).Should().BeNull();
        (await GetPendingAsync()).Should().ContainSingle().Which.GardenId.Should().Be(garden.Id);
    }

    [Fact]
    public async Task DeleteUserDataAsync_DeleteForAllWithoutEmail_DeletesEverythingPhysically()
    {
        var garden = await SeedGardenWithRolesAsync("Slettes for alle",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));

        var result = await _sut.DeleteUserDataAsync(UserId, null, DeleteForAll(garden.Id), Ct);

        result.IsBlocked.Should().BeFalse();
        (await CountGardensAsync()).Should().Be(0);
        (await CountMembershipsAsync(UserId)).Should().Be(0);
        (await CountMembershipsAsync(OtherUserId)).Should().Be(0);
        (await GetPendingAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteUserDataAsync_DeleteForAllAndTransferInSameCall_HandlesEachGardenSeparately()
    {
        var deleted = await SeedGardenWithRolesAsync("A - slettes",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));
        var transferred = await SeedGardenWithRolesAsync("B - overdrages",
            (UserId, true, GardenAccessLevel.Planlaegger, AccessCategory.Administrator),
            (OtherUserId, false, GardenAccessLevel.Planlaegger, AccessCategory.Editor));
        var resolutions = new Dictionary<Guid, GardenOwnershipResolution>
        {
            [deleted.Id] = GardenOwnershipResolution.DeleteForAll(),
            [transferred.Id] = GardenOwnershipResolution.TransferTo(OtherUserId)
        };

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, resolutions, Ct);

        result.IsBlocked.Should().BeFalse();
        (await FindGardenAsync(deleted.Id))!.IsDeleted.Should().BeTrue();
        (await FindGardenAsync(transferred.Id))!.IsDeleted.Should().BeFalse();
        (await FindMembershipAsync(transferred.Id, OtherUserId))!.IsOwner.Should().BeTrue();
        (await FindMembershipAsync(transferred.Id, UserId)).Should().BeNull();
        (await GetPendingAsync()).Should().ContainSingle().Which.GardenId.Should().Be(deleted.Id);
    }

    [Fact]
    public async Task DeleteUserDataAsync_Blocked_CreatesNoPendingRestoreAndChangesNothing()
    {
        var garden = await SeedGardenAsync("Blokeret have", (UserId, true), (OtherUserId, false));

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeTrue();
        (await GetPendingAsync()).Should().BeEmpty();
        (await FindGardenAsync(garden.Id))!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task PendingGardenRestore_IsRemovedWhenGardenIsPhysicallyDeleted()
    {
        var garden = await SeedGardenAsync("Min have", (UserId, true));
        await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        await using (var context = new PlannerDbContext(_options))
        {
            await context.Gardens.IgnoreQueryFilters().Where(g => g.Id == garden.Id).ExecuteDeleteAsync(Ct);
        }

        (await GetPendingAsync()).Should().BeEmpty();
    }
}
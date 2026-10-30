namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using NSubstitute;
using Xunit;

public sealed class AccountDeletionServiceTests : IDisposable
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";
    private const string UserEmail = "bruger@example.dk";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PlannerDbContext> _options;
    private readonly IReAuthFailureTracker _tracker = Substitute.For<IReAuthFailureTracker>();
    private readonly AccountDeletionService _sut;

    public AccountDeletionServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite(_connection).Options;

        using (var context = new PlannerDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _sut = new AccountDeletionService(
            new TestFactory(_options), _tracker, NullLogger<AccountDeletionService>.Instance);
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
    public async Task DeleteUserDataAsync_SoleMember_DeletesGardenAndAllUserData()
    {
        var garden = await SeedGardenAsync("Min have", (UserId, true));
        await SeedEntitlementAsync(garden.Id, UserId);
        await SeedDraftAsync(UserId);

        var result = await _sut.DeleteUserDataAsync(UserId, UserEmail, Ct);

        result.IsBlocked.Should().BeFalse();
        (await CountGardensAsync()).Should().Be(0);
        (await CountMembershipsAsync(UserId)).Should().Be(0);
        (await CountEntitlementsAsync(UserId)).Should().Be(0);
        (await CountDraftsAsync(UserId)).Should().Be(0);
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
}
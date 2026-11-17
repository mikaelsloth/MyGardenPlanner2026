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

public sealed class GardenRestoreServiceTests : IDisposable
{
    private const string FormerUserId = "user-old";
    private const string NewUserId = "user-new";
    private const string OwnerEmail = "bruger@example.dk";

    private static readonly DateTimeOffset Now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PlannerDbContext> _options;
    private readonly GardenRestoreService _sut;

    public GardenRestoreServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite(_connection).Options;

        using (var context = new PlannerDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        _sut = new GardenRestoreService(
            new TestFactory(_options), new FixedTimeProvider(Now), NullLogger<GardenRestoreService>.Instance);
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

    private async Task<(Guid GardenId, Guid PendingId)> SeedDeletedGardenAsync(
        string name = "Slettet have", string email = OwnerEmail,
        DateTimeOffset? expiresUtc = null, bool withEntitlement = true)
    {
        var deletedAt = Now.AddDays(-1);

        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = name, IsDeleted = true, DeletedAtUtc = deletedAt, DeletedBy = FormerUserId };
        context.Gardens.Add(garden);
        context.GardenMemberships.Add(new GardenMembership
        {
            GardenId = garden.Id,
            UserId = FormerUserId,
            IsOwner = true,
            Layer = GardenAccessLevel.Planlaegger,
            Category = AccessCategory.Administrator,
            IsDeleted = true,
            DeletedAtUtc = deletedAt,
            DeletedBy = FormerUserId
        });

        if (withEntitlement)
        {
            context.UserEntitlements.Add(new UserEntitlement
            {
                GardenId = garden.Id,
                UserId = FormerUserId,
                Layer = GardenAccessLevel.Planlaegger,
                Category = AccessCategory.Administrator,
                IsDeleted = true,
                DeletedAtUtc = deletedAt,
                DeletedBy = FormerUserId
            });
        }

        var pending = new PendingGardenRestore
        {
            GardenId = garden.Id,
            OwnerEmailHash = RestoreEmailHasher.Hash(email),
            FormerUserId = FormerUserId,
            DeletedAtUtc = deletedAt,
            ExpiresUtc = expiresUtc ?? Now.AddDays(29)
        };
        context.PendingGardenRestores.Add(pending);

        await context.SaveChangesAsync(Ct);
        return (garden.Id, pending.Id);
    }

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

    private async Task<List<PendingGardenRestore>> GetPendingAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.PendingGardenRestores.ToListAsync(Ct);
    }

    // ---------- GetRestorableGardensAsync ----------

    [Fact]
    public async Task GetRestorableGardensAsync_MatchingEmail_ReturnsGarden()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync("Min have");

        var result = await _sut.GetRestorableGardensAsync(OwnerEmail, Ct);

        var restorable = result.Should().ContainSingle().Subject;
        restorable.PendingRestoreId.Should().Be(pendingId);
        restorable.GardenId.Should().Be(gardenId);
        restorable.Name.Should().Be("Min have");
        restorable.DeletedAtUtc.Should().Be(Now.AddDays(-1));
        restorable.ExpiresUtc.Should().Be(Now.AddDays(29));
    }

    [Fact]
    public async Task GetRestorableGardensAsync_EmailIsCaseAndWhitespaceInsensitive()
    {
        await SeedDeletedGardenAsync();

        var result = await _sut.GetRestorableGardensAsync("  Bruger@Example.DK ", Ct);

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task GetRestorableGardensAsync_OtherEmail_ReturnsEmpty()
    {
        await SeedDeletedGardenAsync();

        var result = await _sut.GetRestorableGardensAsync("anden@example.dk", Ct);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRestorableGardensAsync_ExpiredIsExcluded_ExpiringExactlyNowIsIncluded()
    {
        await SeedDeletedGardenAsync("Udløbet", expiresUtc: Now.AddSeconds(-1));
        await SeedDeletedGardenAsync("Udløber nu", expiresUtc: Now);

        var result = await _sut.GetRestorableGardensAsync(OwnerEmail, Ct);

        result.Select(r => r.Name).Should().Equal("Udløber nu");
    }

    [Fact]
    public async Task GetRestorableGardensAsync_MultipleGardens_AreSortedByName()
    {
        await SeedDeletedGardenAsync("Bagehaven");
        await SeedDeletedGardenAsync("Aftenhaven");

        var result = await _sut.GetRestorableGardensAsync(OwnerEmail, Ct);

        result.Select(r => r.Name).Should().Equal("Aftenhaven", "Bagehaven");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetRestorableGardensAsync_BlankEmail_Throws(string email)
    {
        var act = () => _sut.GetRestorableGardensAsync(email, Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------- RestoreGardenAsync ----------

    [Fact]
    public async Task RestoreGardenAsync_Success_UndeletesGardenAndMovesOwnersRowsToNewUser()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync();

        var restoredId = await _sut.RestoreGardenAsync(pendingId, NewUserId, OwnerEmail, Ct);

        restoredId.Should().Be(gardenId);

        var garden = await FindGardenAsync(gardenId);
        garden!.IsDeleted.Should().BeFalse();
        garden.DeletedAtUtc.Should().BeNull();
        garden.DeletedBy.Should().BeNull();

        var membership = await FindMembershipAsync(gardenId, NewUserId);
        membership.Should().NotBeNull();
        membership!.IsDeleted.Should().BeFalse();
        membership.DeletedAtUtc.Should().BeNull();
        membership.DeletedBy.Should().BeNull();
        membership.IsOwner.Should().BeTrue();
        membership.Layer.Should().Be(GardenAccessLevel.Planlaegger);
        membership.Category.Should().Be(AccessCategory.Administrator);
        (await FindMembershipAsync(gardenId, FormerUserId)).Should().BeNull();

        var entitlement = await FindEntitlementAsync(gardenId, NewUserId);
        entitlement.Should().NotBeNull();
        entitlement!.IsDeleted.Should().BeFalse();
        entitlement.DeletedAtUtc.Should().BeNull();
        entitlement.DeletedBy.Should().BeNull();
        (await FindEntitlementAsync(gardenId, FormerUserId)).Should().BeNull();

        (await GetPendingAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task RestoreGardenAsync_Success_RestoredRowsAreVisibleThroughQueryFilters()
    {
        var (_, pendingId) = await SeedDeletedGardenAsync();

        await _sut.RestoreGardenAsync(pendingId, NewUserId, OwnerEmail, Ct);

        await using var context = new PlannerDbContext(_options);
        (await context.Gardens.CountAsync(Ct)).Should().Be(1);
        (await context.GardenMemberships.CountAsync(m => m.UserId == NewUserId, Ct)).Should().Be(1);
        (await context.UserEntitlements.CountAsync(e => e.UserId == NewUserId, Ct)).Should().Be(1);
    }

    [Fact]
    public async Task RestoreGardenAsync_WithoutEntitlement_StillRestores()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync(withEntitlement: false);

        await _sut.RestoreGardenAsync(pendingId, NewUserId, OwnerEmail, Ct);

        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeFalse();
        (await FindMembershipAsync(gardenId, NewUserId)).Should().NotBeNull();
        (await FindEntitlementAsync(gardenId, NewUserId)).Should().BeNull();
    }

    [Fact]
    public async Task RestoreGardenAsync_EmailIsCaseAndWhitespaceInsensitive()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync();

        var restoredId = await _sut.RestoreGardenAsync(pendingId, NewUserId, " BRUGER@example.dk ", Ct);

        restoredId.Should().Be(gardenId);
    }

    [Fact]
    public async Task RestoreGardenAsync_OnlyRestoresTheChosenGarden()
    {
        var (firstId, firstPending) = await SeedDeletedGardenAsync("A");
        var (secondId, secondPending) = await SeedDeletedGardenAsync("B");

        await _sut.RestoreGardenAsync(firstPending, NewUserId, OwnerEmail, Ct);

        (await FindGardenAsync(firstId))!.IsDeleted.Should().BeFalse();
        (await FindGardenAsync(secondId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(secondId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await GetPendingAsync()).Should().ContainSingle().Which.Id.Should().Be(secondPending);
    }

    [Fact]
    public async Task RestoreGardenAsync_OtherEmail_ThrowsAndChangesNothing()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync();

        var act = () => _sut.RestoreGardenAsync(pendingId, NewUserId, "anden@example.dk", Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task RestoreGardenAsync_UnknownPending_Throws()
    {
        await SeedDeletedGardenAsync();

        var act = () => _sut.RestoreGardenAsync(Guid.NewGuid(), NewUserId, OwnerEmail, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RestoreGardenAsync_Expired_ThrowsAndChangesNothing()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync(expiresUtc: Now.AddSeconds(-1));

        var act = () => _sut.RestoreGardenAsync(pendingId, NewUserId, OwnerEmail, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*udløbet*");
        await AssertNothingRestoredAsync(gardenId);
    }

    [Fact]
    public async Task RestoreGardenAsync_SecondCallForSameRestore_Throws()
    {
        var (_, pendingId) = await SeedDeletedGardenAsync();
        await _sut.RestoreGardenAsync(pendingId, NewUserId, OwnerEmail, Ct);

        var act = () => _sut.RestoreGardenAsync(pendingId, "user-third", OwnerEmail, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RestoreGardenAsync_OwnerMembershipMissing_ThrowsAndRollsBack()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync();
        await using (var context = new PlannerDbContext(_options))
        {
            await context.GardenMemberships.IgnoreQueryFilters()
                .Where(m => m.GardenId == gardenId)
                .ExecuteDeleteAsync(Ct);
        }

        var act = () => _sut.RestoreGardenAsync(pendingId, NewUserId, OwnerEmail, Ct);

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeTrue();
        (await GetPendingAsync()).Should().ContainSingle();
    }

    [Theory]
    [InlineData("", OwnerEmail)]
    [InlineData("  ", OwnerEmail)]
    [InlineData(NewUserId, "")]
    [InlineData(NewUserId, "  ")]
    public async Task RestoreGardenAsync_BlankArguments_Throw(string userId, string email)
    {
        var act = () => _sut.RestoreGardenAsync(Guid.NewGuid(), userId, email, Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ---------- Rundtur: kontosletning -> gendannelse ----------

    [Fact]
    public async Task DeleteAccountThenRestore_RoundTrip_GivesNewUserTheGarden()
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
                Layer = GardenAccessLevel.BedDesigner,
                Category = AccessCategory.Administrator
            });
            context.UserEntitlements.Add(new UserEntitlement { GardenId = garden.Id, UserId = FormerUserId });
            await context.SaveChangesAsync(Ct);
        }

        var restoreOptions = Substitute.For<IOptionsMonitor<GardenRestoreOptions>>();
        restoreOptions.CurrentValue.Returns(new GardenRestoreOptions { RetentionDays = 30 });
        var deletion = new AccountDeletionService(
            new TestFactory(_options), Substitute.For<IReAuthFailureTracker>(), restoreOptions,
            new FixedTimeProvider(Now), NullLogger<AccountDeletionService>.Instance);

        await deletion.DeleteUserDataAsync(FormerUserId, OwnerEmail, Ct);

        var restorable = await _sut.GetRestorableGardensAsync(OwnerEmail, Ct);
        restorable.Should().ContainSingle().Which.GardenId.Should().Be(gardenId);

        await _sut.RestoreGardenAsync(restorable[0].PendingRestoreId, NewUserId, OwnerEmail, Ct);

        await using var verify = new PlannerDbContext(_options);
        (await verify.Gardens.CountAsync(Ct)).Should().Be(1);
        var membership = await verify.GardenMemberships.SingleAsync(Ct);
        membership.UserId.Should().Be(NewUserId);
        membership.Layer.Should().Be(GardenAccessLevel.BedDesigner);
        (await verify.UserEntitlements.CountAsync(e => e.UserId == NewUserId, Ct)).Should().Be(1);
        (await _sut.GetRestorableGardensAsync(OwnerEmail, Ct)).Should().BeEmpty();
    }

    private async Task AssertNothingRestoredAsync(Guid gardenId)
    {
        (await FindGardenAsync(gardenId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(gardenId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await FindMembershipAsync(gardenId, NewUserId)).Should().BeNull();
        (await FindEntitlementAsync(gardenId, FormerUserId))!.IsDeleted.Should().BeTrue();
        (await GetPendingAsync()).Should().ContainSingle();
    }
}
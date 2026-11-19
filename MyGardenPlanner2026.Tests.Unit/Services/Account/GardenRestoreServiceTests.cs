namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using Xunit;

public sealed class GardenRestoreServiceTests : IDisposable
{
    private const string FormerUserId = "user-old";
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

        _sut = new GardenRestoreService(new TestFactory(_options), new FixedTimeProvider(Now));
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

    private async Task<(Guid GardenId, Guid PendingId)> SeedDeletedGardenAsync(
        string name = "Slettet have",
        string email = OwnerEmail,
        DateTimeOffset? expiresUtc = null,
        GardenAccessLevel layer = GardenAccessLevel.BedDesigner,
        AccessCategory category = AccessCategory.Editor,
        bool withOwnerMembership = true)
    {
        var deletedAt = Now.AddDays(-1);

        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = name, IsDeleted = true, DeletedAtUtc = deletedAt, DeletedBy = FormerUserId };
        context.Gardens.Add(garden);

        if (withOwnerMembership)
        {
            context.GardenMemberships.Add(new GardenMembership
            {
                GardenId = garden.Id,
                UserId = FormerUserId,
                IsOwner = true,
                Layer = layer,
                Category = category,
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

    [Fact]
    public async Task GetRestorableGardensAsync_MatchingEmail_ReturnsGardenWithFormerOwnerLevel()
    {
        var (gardenId, pendingId) = await SeedDeletedGardenAsync(
            "Min have", layer: GardenAccessLevel.Planlaegger, category: AccessCategory.Administrator);

        var result = await _sut.GetRestorableGardensAsync(OwnerEmail, Ct);

        var restorable = result.Should().ContainSingle().Subject;
        restorable.PendingRestoreId.Should().Be(pendingId);
        restorable.GardenId.Should().Be(gardenId);
        restorable.Name.Should().Be("Min have");
        restorable.DeletedAtUtc.Should().Be(Now.AddDays(-1));
        restorable.ExpiresUtc.Should().Be(Now.AddDays(29));
        restorable.Layer.Should().Be(GardenAccessLevel.Planlaegger);
        restorable.Category.Should().Be(AccessCategory.Administrator);
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

    [Fact]
    public async Task GetRestorableGardensAsync_TicketWithoutOwnerMembership_IsExcluded()
    {
        await SeedDeletedGardenAsync("Uden ejer", withOwnerMembership: false);
        await SeedDeletedGardenAsync("Med ejer");

        var result = await _sut.GetRestorableGardensAsync(OwnerEmail, Ct);

        result.Select(r => r.Name).Should().Equal("Med ejer");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetRestorableGardensAsync_BlankEmail_Throws(string email)
    {
        var act = () => _sut.GetRestorableGardensAsync(email, Ct);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
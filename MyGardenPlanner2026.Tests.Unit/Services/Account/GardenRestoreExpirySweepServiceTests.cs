namespace MyGardenPlanner2026.Tests.Unit.Services.Account;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.Services.Account;
using NSubstitute;
using System.Text.Json;
using Xunit;

public sealed class GardenRestoreExpirySweepServiceTests : IDisposable
{
    private const string FormerUserId = "user-old";
    private const string ActiveUserId = "user-active";
    private const string OwnerEmail = "bruger@example.dk";

    private static readonly DateTimeOffset Now = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PlannerDbContext> _options;
    private readonly GardenRestoreExpirySweepService _sut;

    public GardenRestoreExpirySweepServiceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite(_connection).Options;

        using (var context = new PlannerDbContext(_options))
        {
            context.Database.EnsureCreated();
        }

        var restoreOptions = Substitute.For<IOptionsMonitor<GardenRestoreOptions>>();
        restoreOptions.CurrentValue.Returns(new GardenRestoreOptions { RetentionDays = 30, SweepIntervalMinutes = 60 });

        _sut = new GardenRestoreExpirySweepService(
            new TestFactory(_options), restoreOptions, new FixedTimeProvider(Now),
            NullLogger<GardenRestoreExpirySweepService>.Instance);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _connection.Dispose();
    }

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

    private async Task<(Guid GardenId, Guid TicketId)> SeedTicketAsync(
        string name, DateTimeOffset expiresUtc, bool gardenIsDeleted = true)
    {
        DateTimeOffset? deletedAt = gardenIsDeleted ? Now.AddDays(-30) : null;
        var deletedBy = gardenIsDeleted ? FormerUserId : null;

        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = name, IsDeleted = gardenIsDeleted, DeletedAtUtc = deletedAt, DeletedBy = deletedBy };
        context.Gardens.Add(garden);
        context.GardenMemberships.Add(new GardenMembership
        {
            GardenId = garden.Id,
            UserId = FormerUserId,
            IsOwner = true,
            Layer = GardenAccessLevel.Planlaegger,
            Category = AccessCategory.Administrator,
            IsDeleted = gardenIsDeleted,
            DeletedAtUtc = deletedAt,
            DeletedBy = deletedBy
        });
        context.UserEntitlements.Add(new UserEntitlement
        {
            GardenId = garden.Id,
            UserId = FormerUserId,
            IsDeleted = gardenIsDeleted,
            DeletedAtUtc = deletedAt,
            DeletedBy = deletedBy
        });

        var ticket = new PendingGardenRestore
        {
            GardenId = garden.Id,
            OwnerEmailHash = RestoreEmailHasher.Hash(OwnerEmail),
            FormerUserId = FormerUserId,
            DeletedAtUtc = Now.AddDays(-30),
            ExpiresUtc = expiresUtc
        };
        context.PendingGardenRestores.Add(ticket);

        await context.SaveChangesAsync(Ct);
        return (garden.Id, ticket.Id);
    }

    private async Task<Guid> SeedActiveGardenAsync()
    {
        await using var context = new PlannerDbContext(_options);
        var garden = new Garden { Name = "Aktiv have" };
        context.Gardens.Add(garden);
        context.GardenMemberships.Add(new GardenMembership { GardenId = garden.Id, UserId = ActiveUserId, IsOwner = true });
        context.UserEntitlements.Add(new UserEntitlement { GardenId = garden.Id, UserId = ActiveUserId });
        await context.SaveChangesAsync(Ct);
        return garden.Id;
    }

    private async Task<int> CountGardensAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.Gardens.IgnoreQueryFilters().CountAsync(Ct);
    }

    private async Task<int> CountMembershipsAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.GardenMemberships.IgnoreQueryFilters().CountAsync(Ct);
    }

    private async Task<int> CountEntitlementsAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.UserEntitlements.IgnoreQueryFilters().CountAsync(Ct);
    }

    private async Task<List<PendingGardenRestore>> GetTicketsAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.PendingGardenRestores.ToListAsync(Ct);
    }

    // ---------- SweepOnceAsync ----------

    [Fact]
    public async Task SweepOnceAsync_NoTickets_ReturnsZero()
    {
        var activeId = await SeedActiveGardenAsync();

        var result = await _sut.SweepOnceAsync(Ct);

        result.Should().Be(0);
        (await CountGardensAsync()).Should().Be(1);
        activeId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SweepOnceAsync_ExpiredTicket_PurgesGardenOwnerRowsAndTicket()
    {
        await SeedTicketAsync("Udløbet", Now.AddSeconds(-1));

        var result = await _sut.SweepOnceAsync(Ct);

        result.Should().Be(1);
        (await CountGardensAsync()).Should().Be(0);
        (await CountMembershipsAsync()).Should().Be(0);
        (await CountEntitlementsAsync()).Should().Be(0);
        (await GetTicketsAsync()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(86400)]
    public async Task SweepOnceAsync_TicketNotYetExpired_IsKept(int secondsAfterNow)
    {
        var (gardenId, ticketId) = await SeedTicketAsync("Stadig gendannelig", Now.AddSeconds(secondsAfterNow));

        var result = await _sut.SweepOnceAsync(Ct);

        result.Should().Be(0);
        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync()).Should().Be(1);
        (await CountEntitlementsAsync()).Should().Be(1);
        (await GetTicketsAsync()).Should().ContainSingle().Which.Id.Should().Be(ticketId);
        gardenId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SweepOnceAsync_MixedTickets_OnlyExpiredAreRemoved()
    {
        await SeedTicketAsync("Udløbet 1", Now.AddDays(-1));
        await SeedTicketAsync("Udløbet 2", Now.AddSeconds(-1));
        var (keptGardenId, keptTicketId) = await SeedTicketAsync("Gendannelig", Now.AddDays(5));

        var result = await _sut.SweepOnceAsync(Ct);

        result.Should().Be(2);
        (await CountGardensAsync()).Should().Be(1);
        (await GetTicketsAsync()).Should().ContainSingle().Which.Id.Should().Be(keptTicketId);

        await using var context = new PlannerDbContext(_options);
        (await context.Gardens.IgnoreQueryFilters().SingleAsync(Ct)).Id.Should().Be(keptGardenId);
    }

    [Fact]
    public async Task SweepOnceAsync_DoesNotTouchActiveGardens()
    {
        var activeId = await SeedActiveGardenAsync();
        await SeedTicketAsync("Udløbet", Now.AddDays(-1));

        await _sut.SweepOnceAsync(Ct);

        await using var context = new PlannerDbContext(_options);
        var garden = await context.Gardens.SingleAsync(Ct);
        garden.Id.Should().Be(activeId);
        (await context.GardenMemberships.CountAsync(m => m.UserId == ActiveUserId, Ct)).Should().Be(1);
        (await context.UserEntitlements.CountAsync(e => e.UserId == ActiveUserId, Ct)).Should().Be(1);
    }

    [Fact]
    public async Task SweepOnceAsync_ExpiredTicketForGardenNotMarkedDeleted_KeepsGardenAndRemovesTicket()
    {
        // Fx en have, der blev gendannet, mens sweep'et kørte: haven må aldrig slettes.
        await SeedTicketAsync("Gendannet", Now.AddDays(-1), gardenIsDeleted: false);

        var result = await _sut.SweepOnceAsync(Ct);

        result.Should().Be(1);
        (await CountGardensAsync()).Should().Be(1);
        (await CountMembershipsAsync()).Should().Be(1);
        (await CountEntitlementsAsync()).Should().Be(1);
        (await GetTicketsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task SweepOnceAsync_SecondRun_IsIdempotent()
    {
        await SeedTicketAsync("Udløbet", Now.AddDays(-1));
        await _sut.SweepOnceAsync(Ct);

        var result = await _sut.SweepOnceAsync(Ct);

        result.Should().Be(0);
        (await CountGardensAsync()).Should().Be(0);
    }

    // ---------- ExecuteAsync ----------

    [Fact]
    public async Task ExecuteAsync_SweepsOnStartAndStopsOnCancellation()
    {
        await SeedTicketAsync("Udløbet", Now.AddDays(-1));

        await _sut.StartAsync(Ct);
        try
        {
            for (var attempt = 0; attempt < 50 && await CountGardensAsync() > 0; attempt++)
            {
                await Task.Delay(100, Ct);
            }

            (await CountGardensAsync()).Should().Be(0);
            (await GetTicketsAsync()).Should().BeEmpty();
        }
        finally
        {
            await _sut.StopAsync(CancellationToken.None);
        }
    }

    private async Task<List<AuditLog>> GetAuditAsync()
    {
        await using var context = new PlannerDbContext(_options);
        return await context.AuditLogs.AsNoTracking().OrderBy(a => a.Id).ToListAsync(Ct);
    }

    [Fact]
    public async Task SweepOnceAsync_PurgedGarden_WritesSystemAuditRow()
    {
        var (gardenId, _) = await SeedTicketAsync("Udløbet", Now.AddSeconds(-1));

        await _sut.SweepOnceAsync(Ct);

        var row = (await GetAuditAsync()).Should().ContainSingle().Subject;
        row.EntityName.Should().Be("Garden");
        row.Action.Should().Be(AuditAction.Delete);
        row.EntityId.Should().Be(gardenId.ToString());
        row.UserId.Should().BeNull();
        row.UserEmail.Should().BeNull();
        row.IpAddress.Should().BeNull();
        row.TimestampUtc.Should().Be(Now);
        row.NewValues.Should().BeNull();
        JsonElement.Parse(row.OldValues!).GetProperty("RestoreExpiredUtc").ToString()
            .Should().Contain("2026-11-20");
    }

    [Fact]
    public async Task SweepOnceAsync_OnlyExpiredGardensAreAudited()
    {
        var (expiredId, _) = await SeedTicketAsync("Udløbet", Now.AddDays(-1));
        await SeedTicketAsync("Gendannelig", Now.AddDays(5));

        await _sut.SweepOnceAsync(Ct);

        (await GetAuditAsync()).Should().ContainSingle().Which.EntityId.Should().Be(expiredId.ToString());
    }

    [Fact]
    public async Task SweepOnceAsync_ExpiredTicketForGardenNotMarkedDeleted_WritesNoAuditRow()
    {
        await SeedTicketAsync("Gendannet", Now.AddDays(-1), gardenIsDeleted: false);

        await _sut.SweepOnceAsync(Ct);

        (await GetAuditAsync()).Should().BeEmpty();
    }
}
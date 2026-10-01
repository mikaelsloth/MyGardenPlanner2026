namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using Xunit;

public sealed class SqliteAdminDbContextFactoryTests : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly SqliteAdminDbContextFactory sut;

    public SqliteAdminDbContextFactoryTests()
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        sut = new SqliteAdminDbContextFactory(connection);
        using var context = sut.CreateDbContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => connection.Dispose();

    private static AuditLog NewLog(string entityId) => new()
    {
        EntityName = "Test",
        EntityId = entityId,
        Action = AuditAction.Create,
        TimestampUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task CreateDbContext_ReturnsNewInstanceOnEveryCall()
    {
        var first = await sut.CreateDbContextAsync(TestContext.Current.CancellationToken);
        var second = await sut.CreateDbContextAsync(TestContext.Current.CancellationToken);

        first.Should().NotBeSameAs(second);
    }

    [Fact]
    public async Task CreateDbContext_ContextsShareTheSameInMemoryDatabase()
    {
        using var writer = await sut.CreateDbContextAsync(TestContext.Current.CancellationToken);
        {
            await writer.AuditLogs.AddAsync(NewLog("shared"), TestContext.Current.CancellationToken);
            await writer.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        using var reader = await sut.CreateDbContextAsync(TestContext.Current.CancellationToken);
        var exists = await reader.AuditLogs.AnyAsync(l => l.EntityId == "shared", TestContext.Current.CancellationToken);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CreateDbContextAsync_ReturnsUsableContextOnTheSameDatabase()
    {
        using var writer = await sut.CreateDbContextAsync(TestContext.Current.CancellationToken);
        {
            await writer.AuditLogs.AddAsync(NewLog("async"), TestContext.Current.CancellationToken);
            await writer.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var reader = await sut.CreateDbContextAsync(TestContext.Current.CancellationToken);
        var count = await reader.AuditLogs.CountAsync(TestContext.Current.CancellationToken);

        count.Should().Be(1);
    }
}
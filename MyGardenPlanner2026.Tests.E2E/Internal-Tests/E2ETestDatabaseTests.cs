namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Infrastructure.Data;

public sealed class E2ETestDatabaseTests
{
    [Fact]
    public async Task CreateAsync_OpretterMigreretDatabase_SomKanForbindesTil()
    {
        await using var database = await E2ETestDatabase.CreateAsync();
        await using var context = new PlannerDbContext(
            E2ETestDatabase.CreateContextOptions(E2ESqlEnvironment.MigrationConnectionString(database.Name)));

        (await context.Database.CanConnectAsync(TestContext.Current.CancellationToken)).Should().BeTrue();
        (await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken)).Should().NotBeEmpty();
    }

    [Fact]
    public async Task DisposeAsync_Lokalt_FjernerDatabasen()
    {
        Assert.SkipWhen(E2ESqlEnvironment.IsCi, "Kun relevant lokalt: i CI droppes databasen først ved næste CreateAsync.");

        var database = await E2ETestDatabase.CreateAsync();
        await database.DisposeAsync();

        await using var context = new PlannerDbContext(
            E2ETestDatabase.CreateContextOptions(E2ESqlEnvironment.MigrationConnectionString(database.Name)));

        (await context.Database.CanConnectAsync(TestContext.Current.CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task DropIfExistsAsync_UdenEksisterendeDatabase_KasterIkke()
    {
        var act = () => E2ETestDatabase.DropIfExistsAsync($"MyGardenPlanner2026_E2E_{Guid.NewGuid():N}");

        await act.Should().NotThrowAsync();
    }
}
namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Infrastructure.Data;
using Xunit;

public class PlannerDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PlannerDbContext> _contextOptions;

    public PlannerDbContextTests()
    {
        // Opret in-memory SQLite forbindelse
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new PlannerDbContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CanInsertAndRetrievePlant()
    {
        // Arrange
        using var context = new PlannerDbContext(_contextOptions);
        var plant = new Plant { Name = "Rose", Species = "Rosa" };

        // Act
        context.Plants.Add(plant);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        using var verifyContext = new PlannerDbContext(_contextOptions);
        var savedPlant = await verifyContext.Plants.FirstOrDefaultAsync(p => p.Name == "Rose", cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedPlant);
        Assert.Equal("Rosa", savedPlant.Species);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
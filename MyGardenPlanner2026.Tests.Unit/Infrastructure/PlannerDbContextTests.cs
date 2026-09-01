namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Tests.Unit;
using Xunit;

public class PlannerDbContextTests : TestDbContext
{
    [Fact]
    public async Task CanInsertAndRetrievePlant()
    {
        // Arrange
        using var context = CreateDbContext();
        var plant = new DummyEntityPlant { Name = "Rose", Species = "Rosa" };

        // Act
        await context.Plants.AddAsync(plant, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        using var verifyContext = CreateDbContext();
        var savedPlant = await verifyContext.Plants.FirstOrDefaultAsync(p => p.Name == "Rose", cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(savedPlant);
        Assert.Equal("Rosa", savedPlant.Species);
    }
}
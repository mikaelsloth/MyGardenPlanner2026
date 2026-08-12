namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using Xunit;

public class SubscriptionTierPersistenceTests : TestDbContext
{
    [Fact]
    public async Task CanInsertAndRetrieveSubscriptionTier_WithFeatureLimitsAndIncludedFeatures()
    {
        using var context = CreateDbContext();
        var tier = new SubscriptionTier
        {
            Level = GardenAccessLevel.Planlaegger,
            AccessCategory = AccessCategory.Administrator,
            Name = "Planlægger · Administrator",
            Description = "Fuld adgang til planlægning.",
            AnnualPrice = 192m,
            MonthlyPrice = 16m,
            PerpetualPrice = 480m,
            IncludedFeatures = ["50 planlagte bede pr. have"],
            FeatureLimits = new Dictionary<string, string> { ["Planlagte bede"] = "50" }
        };

        context.Add(tier);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        using var verifyContext = CreateDbContext();
        var saved = await verifyContext.Set<SubscriptionTier>()
            .FirstOrDefaultAsync(t => t.Name == "Planlægger · Administrator", TestContext.Current.CancellationToken);

        saved.Should().NotBeNull();
        saved!.IncludedFeatures.Should().ContainSingle().Which.Should().Be("50 planlagte bede pr. have");
        saved.FeatureLimits.Should().ContainKey("Planlagte bede").WhoseValue.Should().Be("50");
    }

    [Fact]
    public async Task CannotInsertDuplicate_LevelAndAccessCategoryCombination()
    {
        using var context = CreateDbContext();
        context.AddRange(
            new SubscriptionTier { Level = GardenAccessLevel.HaveArkitekt, AccessCategory = AccessCategory.Viewer, Name = "A", AnnualPrice = 42m, MonthlyPrice = 3.5m, PerpetualPrice = 105m },
            new SubscriptionTier { Level = GardenAccessLevel.HaveArkitekt, AccessCategory = AccessCategory.Viewer, Name = "B", AnnualPrice = 42m, MonthlyPrice = 3.5m, PerpetualPrice = 105m });

        var act = async () => await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
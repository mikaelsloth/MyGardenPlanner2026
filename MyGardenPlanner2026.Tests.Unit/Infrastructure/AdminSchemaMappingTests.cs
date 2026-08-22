namespace MyGardenPlanner2026.Tests.Unit.Infrastructure;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;
using Xunit;

/// <summary>
/// Verificerer kun modelbygningen (schema-mapping) — opretter aldrig en reel forbindelse.
/// UseSqlServer kræver en gyldig connection-streng-syntaks, men EF Core forsøger ikke at
/// åbne forbindelsen, når vi udelukkende inspicerer context.Model.
/// </summary>
public class AdminSchemaMappingTests
{
    [Theory]
    [InlineData(typeof(SubscriptionTier), "SubscriptionTiers")]
    [InlineData(typeof(GardenVolumeDiscountTier), "GardenVolumeDiscountTiers")]
    [InlineData(typeof(SubscriptionAddOn), "SubscriptionAddOns")]
    public void ProtectedEntities_AreMappedToAdminSchema(Type entityType, string expectedTableName)
    {
        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlServer("Server=.;Database=ModelOnly;Trusted_Connection=True;")
            .Options;
        using var context = new PlannerDbContext(options);

        var entity = context.Model.FindEntityType(entityType);

        entity.Should().NotBeNull();
        entity!.GetSchema().Should().Be(PlannerDbContext.AdminSchema);
        entity.GetTableName().Should().Be(expectedTableName);
    }
}
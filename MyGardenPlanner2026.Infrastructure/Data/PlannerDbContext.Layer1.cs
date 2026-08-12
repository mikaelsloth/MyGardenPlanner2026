namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Layer1;

public partial class PlannerDbContext
{
    public DbSet<SubscriptionTier> SubscriptionTiers => Set<SubscriptionTier>();

    private static void ConfigureLayer1(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionTier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.PrimitiveCollection(e => e.IncludedFeatures);

            entity.Property(e => e.FeatureLimits)
                  .HasConversion(JsonContentConverter)
                  .Metadata.SetValueComparer(FeatureLimitsComparer);

            entity.HasIndex(e => new { e.Level, e.AccessCategory }).IsUnique();
        });
    }
}
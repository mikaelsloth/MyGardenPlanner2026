namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Layer1;

public partial class PlannerDbContext
{
    public DbSet<SubscriptionTier> SubscriptionTiers => Set<SubscriptionTier>();
    public DbSet<GardenVolumeDiscountTier> GardenVolumeDiscountTiers => Set<GardenVolumeDiscountTier>();
    public DbSet<SubscriptionAddOn> SubscriptionAddOns => Set<SubscriptionAddOn>();

    private static void ConfigureLayer1(ModelBuilder modelBuilder)
    {
        SubscriptionTierConfig(modelBuilder);
        GardenVolumeDiscountTierConfig(modelBuilder);
        SubscriptionAddOnConfig(modelBuilder);
    }

    private static void SubscriptionAddOnConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionAddOn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.UnitDescription).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Type).IsUnique();
        });
    }

    private static void GardenVolumeDiscountTierConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GardenVolumeDiscountTier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MinGardens).IsUnique();
        });
    }

    private static void SubscriptionTierConfig(ModelBuilder modelBuilder)
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
namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Layer1;

public partial class PlannerDbContext
{
    public const string AdminSchema = "admin";

    public DbSet<SubscriptionTier> SubscriptionTiers => Set<SubscriptionTier>();
    public DbSet<GardenVolumeDiscountTier> GardenVolumeDiscountTiers => Set<GardenVolumeDiscountTier>();
    public DbSet<SubscriptionAddOn> SubscriptionAddOns => Set<SubscriptionAddOn>();

    /// <summary>
    /// Temporal Tables understøttes kun af SQL Server. På SQLite (unit-tests) springes
    /// IsTemporal() over — schema, nøgler og øvrig konfiguration anvendes uændret.
    /// Instansmetode (ikke static), da Database.IsSqlServer() kræver context-instansen.
    /// </summary>
    private void ConfigureLayer1(ModelBuilder modelBuilder)
    {
        var useTemporalTables = Database.IsSqlServer();

        SubscriptionTierConfig(modelBuilder, useTemporalTables);
        GardenVolumeDiscountTierConfig(modelBuilder, useTemporalTables);
        SubscriptionAddOnConfig(modelBuilder, useTemporalTables);
    }

    private static void SubscriptionAddOnConfig(ModelBuilder modelBuilder, bool useTemporalTables)
    {
        modelBuilder.Entity<SubscriptionAddOn>(entity =>
        {
            entity.ToTable("SubscriptionAddOns", AdminSchema, b =>
            {
                if (useTemporalTables)
                {
                    b.IsTemporal(t =>
                    {
                        t.HasPeriodStart("ValidFromUtc");
                        t.HasPeriodEnd("ValidToUtc");
                    });
                }
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.UnitDescription).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Type).IsUnique();
        });
    }

    private static void GardenVolumeDiscountTierConfig(ModelBuilder modelBuilder, bool useTemporalTables)
    {
        modelBuilder.Entity<GardenVolumeDiscountTier>(entity =>
        {
            entity.ToTable("GardenVolumeDiscountTiers", AdminSchema, b =>
            {
                if (useTemporalTables)
                {
                    b.IsTemporal(t =>
                    {
                        t.HasPeriodStart("ValidFromUtc");
                        t.HasPeriodEnd("ValidToUtc");
                    });
                }
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.MinGardens).IsUnique();
        });
    }

    private static void SubscriptionTierConfig(ModelBuilder modelBuilder, bool useTemporalTables)
    {
        modelBuilder.Entity<SubscriptionTier>(entity =>
        {
            entity.ToTable("SubscriptionTiers", AdminSchema, b =>
            {
                if (useTemporalTables)
                {
                    b.IsTemporal(t =>
                    {
                        t.HasPeriodStart("ValidFromUtc");
                        t.HasPeriodEnd("ValidToUtc");
                    });
                }
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
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
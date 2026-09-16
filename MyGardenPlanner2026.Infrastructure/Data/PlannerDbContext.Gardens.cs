namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Gardens;

public partial class PlannerDbContext
{
    public DbSet<Garden> Gardens => Set<Garden>();
    public DbSet<GardenMembership> GardenMemberships => Set<GardenMembership>();
    public DbSet<UserEntitlement> UserEntitlements => Set<UserEntitlement>();
    public DbSet<GardenInvitation> GardenInvitations => Set<GardenInvitation>();
    public DbSet<CheckoutDraft> CheckoutDrafts => Set<CheckoutDraft>();

    /// <summary>
    /// Gardens-entiteterne ligger i standard (dbo) schema og bruger IKKE temporal tables
    /// — modsat admin.*-sikkerhedsentiteterne (se ConfigureAdmin). Almindelig
    /// forretningsdata, ikke §3.2-beskyttet sikkerhedspolicy.
    /// </summary>
    private static void ConfigureGardens(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Garden>(entity =>
        {
            entity.ToTable("Gardens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasMany(e => e.Members)
                  .WithOne(m => m.Garden)
                  .HasForeignKey(m => m.GardenId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GardenMembership>(entity =>
        {
            entity.ToTable("GardenMemberships");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.GardenId, e.UserId }).IsUnique();
        });

        modelBuilder.Entity<UserEntitlement>(entity =>
        {
            entity.ToTable("UserEntitlements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.HasOne(e => e.Garden)
                  .WithMany()
                  .HasForeignKey(e => e.GardenId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.GardenId }).IsUnique();
        });

        modelBuilder.Entity<GardenInvitation>(entity =>
        {
            entity.ToTable("GardenInvitations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.InvitedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(64);
            entity.HasOne(e => e.Garden)
                  .WithMany()
                  .HasForeignKey(e => e.GardenId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.TokenHash).IsUnique();
        });

        modelBuilder.Entity<CheckoutDraft>(entity =>
        {
            entity.ToTable("CheckoutDrafts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.GardenName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.AddOnQuantities)
                              .HasConversion(AddOnQuantitiesConverter)
                              .Metadata.SetValueComparer(AddOnQuantitiesComparer);
            entity.HasIndex(e => e.ExpiresUtc);
        });
    }
}
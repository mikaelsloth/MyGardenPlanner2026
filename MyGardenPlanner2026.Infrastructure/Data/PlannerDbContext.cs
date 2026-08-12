namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities;

public partial class PlannerDbContext(DbContextOptions<PlannerDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<DummyEntityPlant> Plants => Set<DummyEntityPlant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // VIGTIG: Skal kaldes for at konfigurere Identity-tabellerne!

        ConfigureLayer1(modelBuilder);

        modelBuilder.Entity<DummyEntityPlant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Species).HasMaxLength(100);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder, nameof(configurationBuilder));

        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<decimal?>().HavePrecision(18, 2);
    }
}
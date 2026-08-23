namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Linq.Expressions;

public partial class PlannerDbContext(DbContextOptions<PlannerDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<DummyEntityPlant> Plants => Set<DummyEntityPlant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // VIGTIG: Skal kaldes for at konfigurere Identity-tabellerne!

        ConfigureLayer1(modelBuilder);
        ConfigureAdmin(modelBuilder);
        ApplySoftDeleteQueryFilters(modelBuilder);

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

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var condition = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;

public partial class PlannerDbContext
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    private static void ConfigureAdmin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", AdminSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserEmail).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasIndex(e => new { e.EntityName, e.EntityId });
            entity.HasIndex(e => e.TimestampUtc);
        });
    }
}
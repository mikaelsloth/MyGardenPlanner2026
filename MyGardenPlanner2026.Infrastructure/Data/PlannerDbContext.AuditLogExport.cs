namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;

public partial class PlannerDbContext
{
    public DbSet<AuditLogExportJob> AuditLogExportJobs => Set<AuditLogExportJob>();

    /// <summary>
    /// Ligger i admin-schema (skrives kun via IAdminDbContextFactory), men bruger IKKE
    /// temporal tables — transient jobdata med store blobs, ikke en §3.2-policy.
    /// </summary>
    private static void ConfigureAuditLogExportJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogExportJob>(entity =>
        {
            entity.ToTable("AuditLogExportJobs", AdminSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RequestedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.FilterJson).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(260);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(e => new { e.RequestedByUserId, e.Status });
        });
    }
}
namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;

public partial class PlannerDbContext
{
    public DbSet<AuditLogViewerPreference> AuditLogViewerPreferences => Set<AuditLogViewerPreference>();

    /// <summary>
    /// Placeres bevidst i standard (dbo) schema — ikke AdminSchema. Denne tabel
    /// er ikke en §3.2-beskyttet sikkerhedspolicy-entity og kræver hverken
    /// temporal tables eller den begrænsede admin-databasebruger.
    /// </summary>
    private static void ConfigureAuditLogViewerPreference(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogViewerPreference>(entity =>
        {
            entity.ToTable("AuditLogViewerPreferences");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(450);
        });
    }
}
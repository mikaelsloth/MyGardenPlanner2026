namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;

public partial class PlannerDbContext
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RoleElevationRequest> RoleElevationRequests => Set<RoleElevationRequest>();
    public DbSet<ReAuthFailureAttempt> ReAuthFailureAttempts => Set<ReAuthFailureAttempt>();

    /// <summary>
    /// Instansmetode (var static): Database.IsSqlServer() kræver context-instansen,
    /// samme mønster som ConfigureLayer1.
    /// </summary>
    private void ConfigureAdmin(ModelBuilder modelBuilder)
    {
        var useTemporalTables = Database.IsSqlServer();

        ConfigureAuditLog(modelBuilder);
        ConfigureRoleElevation(modelBuilder, useTemporalTables);
        ConfigureReAuthFailureAttempts(modelBuilder);
    }

    private static void ConfigureReAuthFailureAttempts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReAuthFailureAttempt>(entity =>
        {
            entity.ToTable("ReAuthFailureAttempts", AdminSchema);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasIndex(e => new { e.UserId, e.OccurredAtUtc });
        });
    }

    private static void ConfigureRoleElevation(ModelBuilder modelBuilder, bool useTemporalTables)
    {
        modelBuilder.Entity<RoleElevationRequest>(entity =>
        {
            // OBS: Period-kolonnerne kan IKKE hedde "ValidFromUtc"/"ValidToUtc" — de navne er
            // allerede brugt af entitetens egne forretningsfelter (eskaleringsvinduet).
            // "Sys"-præfiks bruges til systemversionerings-perioden i stedet.
            entity.ToTable("RoleElevationRequests", AdminSchema, b =>
            {
                if (useTemporalTables)
                {
                    b.IsTemporal(t =>
                    {
                        t.HasPeriodStart("SysValidFromUtc");
                        t.HasPeriodEnd("SysValidToUtc");
                    });
                }
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RequesterUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ApproverUserId).HasMaxLength(450);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.DeletedBy).HasMaxLength(450);
            entity.HasIndex(e => new { e.RequesterUserId, e.Status });
            entity.HasIndex(e => new { e.RoleName, e.Status });
        });
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
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
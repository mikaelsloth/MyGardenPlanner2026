namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Engangsdatabase til E2E-tests. CreateAsync provisionerer (kun CI), migrerer og
/// begrænser AuditLogs (kun CI). DisposeAsync dropper databasen lokalt; i CI røres den
/// ikke (den droppes ved næste CreateAsync via CiSqlProvisioner).
/// </summary>
public sealed class E2ETestDatabase : IAsyncDisposable
{
    private E2ETestDatabase(string name) => Name = name;

    public string Name { get; }
    public string AppConnectionString => E2ESqlEnvironment.AppConnectionString(Name);
    public string AdminConnectionString => E2ESqlEnvironment.AdminConnectionString(Name);

    public static async Task<E2ETestDatabase> CreateAsync()
    {
        var name = E2ESqlEnvironment.ResolveDatabaseName();

        if (E2ESqlEnvironment.IsCi)
        {
            await CiSqlProvisioner.ProvisionDatabaseAndUsersAsync(name);
        }

        await using (var context = new PlannerDbContext(
                         CreateContextOptions(E2ESqlEnvironment.MigrationConnectionString(name))))
        {
            await context.Database.MigrateAsync();
        }

        if (E2ESqlEnvironment.IsCi)
        {
            await CiSqlProvisioner.RestrictAuditLogsAsync();
        }

        return new E2ETestDatabase(name);
    }

    public static DbContextOptions<PlannerDbContext> CreateContextOptions(string connectionString)
    {
        var builder = new DbContextOptionsBuilder<PlannerDbContext>();
        ConfigureContextOptions(builder, connectionString);
        return builder.Options;
    }

    /// <summary>Fælles opsætning til både direkte DbContext-options og AddDbContext.</summary>
    public static void ConfigureContextOptions(DbContextOptionsBuilder builder, string connectionString) =>
        builder
            .UseSqlServer(connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

    public static async Task DropIfExistsAsync(string databaseName)
    {
        await using var connection = new SqlConnection(E2ESqlEnvironment.MasterConnectionString());
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            $"IF DB_ID('{databaseName}') IS NOT NULL BEGIN " +
            $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
            $"DROP DATABASE [{databaseName}]; END", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (E2ESqlEnvironment.IsCi)
        {
            return;
        }

        await DropIfExistsAsync(Name);
    }
}
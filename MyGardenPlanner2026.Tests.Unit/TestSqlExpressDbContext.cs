namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Basisklasse til SQL Server-integrationstests af Temporal Tables, migrationer og historik.
/// Kræver en reel SQL Express/LocalDB-instans. Opretter en unik testdatabase via rigtige
/// migrationer (Database.Migrate) og sletter den igen ved Dispose.
///
/// Konfiguration: sæt miljøvariablen MGP_TEST_SQLSERVER_CONNECTION, eller opret en lokal,
/// git-ignoreret fil appsettings.Tests.SqlServer.json med nøglen "TestSqlServerConnection".
/// </summary>
public abstract class TestSqlExpressDbContext : IDisposable
{
    private readonly string _databaseName = $"MyGardenPlanner2026_Test_{Guid.NewGuid():N}";
    private readonly DbContextOptions<PlannerDbContext> _contextOptions;
    private bool _databaseCreated;

    protected TestSqlExpressDbContext()
    {
        var baseConnectionString = ResolveBaseConnectionString();
        var connectionString = $"{baseConnectionString};Database={_databaseName}";

        _contextOptions = new DbContextOptionsBuilder<PlannerDbContext>()
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .UseSqlServer(connectionString)
            .EnableSensitiveDataLogging()
            .Options;
    }

    private static string ResolveBaseConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("MGP_TEST_SQLSERVER_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Tests.SqlServer.json", optional: true)
            .Build();

        var fromFile = config["TestSqlServerConnection"];
        return !string.IsNullOrWhiteSpace(fromFile)
            ? fromFile
            : throw new InvalidOperationException(
            "Ingen SQL Server test-connection fundet. Sæt miljøvariablen " +
            "MGP_TEST_SQLSERVER_CONNECTION eller opret appsettings.Tests.SqlServer.json " +
            "med nøglen 'TestSqlServerConnection'.");
    }

    protected PlannerDbContext CreateDbContext()
    {
        var context = new PlannerDbContext(_contextOptions);
        if (!_databaseCreated)
        {
            context.Database.Migrate();
            _databaseCreated = true;
        }
        return context;
    }

    public void Dispose()
    {
        if (_databaseCreated)
        {
            using var context = new PlannerDbContext(_contextOptions);
            context.Database.EnsureDeleted();
        }
        GC.SuppressFinalize(this);
    }
}
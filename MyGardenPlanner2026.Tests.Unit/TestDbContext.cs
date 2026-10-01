namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Infrastructure.Data;
using System;

public abstract class TestDbContext : IDisposable
{
    private readonly SqliteConnection _connection;

    protected TestDbContext()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        using var context = new PlannerDbContext(BuildOptions());
        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Fælles options for alle kontekster i en test: samme åbne in-memory-forbindelse,
    /// sensitive data logging og konsol-logging. Interceptors tilføjes kun, hvis angivet.
    /// </summary>
    private DbContextOptions<PlannerDbContext> BuildOptions(params IInterceptor[] interceptors)
    {
        var builder = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information);

        if (interceptors.Length > 0)
        {
            builder.AddInterceptors(interceptors);
        }

        return builder.Options;
    }

    protected IDbContextFactory<PlannerDbContext> CreateDbContextFactory() =>
        new PooledDbContextFactory<PlannerDbContext>(BuildOptions());

    protected IAdminDbContextFactory CreateAdminDbContextFactory() =>
        CreateAdminDbContextFactoryWithInterceptors();

    protected IAdminDbContextFactory CreateAdminDbContextFactoryWithInterceptors(params IInterceptor[] interceptors) =>
        new AdminDbContextFactory(new PooledDbContextFactory<PlannerDbContext>(BuildOptions(interceptors)));

    protected PlannerDbContext CreateDbContext() => new(BuildOptions());

    /// <summary>Bruges til at teste SaveChangesInterceptors (fx SoftDeleteInterceptor) mod SQLite.</summary>
    protected PlannerDbContext CreateDbContextWithInterceptors(params IInterceptor[] interceptors) =>
        new(BuildOptions(interceptors));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
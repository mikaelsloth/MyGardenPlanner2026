namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Infrastructure.Data;
using System;

public abstract class TestDbContext : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<PlannerDbContext> _contextOptions;

    protected TestDbContext()
    {
        // Opret in-memory SQLite forbindelse
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<PlannerDbContext>()
          .EnableSensitiveDataLogging()
          .LogTo(Console.WriteLine, LogLevel.Information)
          .UseSqlite(_connection)
            .Options;

        using var context = new PlannerDbContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    protected IDbContextFactory<PlannerDbContext> CreateDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;
        return new PooledDbContextFactory<PlannerDbContext>(options);
    }

    protected IAdminDbContextFactory CreateAdminDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        var pooled = new PooledDbContextFactory<PlannerDbContext>(options);
        return new AdminDbContextFactory(pooled);
    }

    protected PlannerDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;
        return new(options);
    }
    public void Dispose()
    {
        _connection.Dispose();
    }
}
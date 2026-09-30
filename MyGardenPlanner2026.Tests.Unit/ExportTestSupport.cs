namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Data;

internal sealed class ExportTestTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class ExportTestOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    where T : class
{
    public T CurrentValue => value;

    public T Get(string? name) => value;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}

internal sealed class ExportTestAdminDbFactory(SqliteConnection connection) : IAdminDbContextFactory
{
    private readonly DbContextOptions<PlannerDbContext> options =
        new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

    public PlannerDbContext CreateDbContext() => new(options);

    public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());
}
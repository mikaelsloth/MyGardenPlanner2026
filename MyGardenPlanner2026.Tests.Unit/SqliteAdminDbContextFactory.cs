namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Infrastructure.Data;

internal sealed class SqliteAdminDbContextFactory(SqliteConnection connection) : IAdminDbContextFactory, IAsyncDisposable
{
    private readonly DbContextOptions<PlannerDbContext> options =
        new DbContextOptionsBuilder<PlannerDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

    public PlannerDbContext CreateDbContext() => new(options);

    public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());

    public ValueTask DisposeAsync() => connection.DisposeAsync();
}
namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

public sealed class AdminDbContextFactory(IDbContextFactory<PlannerDbContext> inner) : IAdminDbContextFactory
{
    public PlannerDbContext CreateDbContext() => inner.CreateDbContext();

    public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        inner.CreateDbContextAsync(cancellationToken);
}
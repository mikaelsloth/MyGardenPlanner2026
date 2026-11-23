namespace MyGardenPlanner2026.Tests.Unit;

using MyGardenPlanner2026.Infrastructure.Data;

public sealed class FakeAdminDbContextFactory(Func<PlannerDbContext> create) : IAdminDbContextFactory
{
    public PlannerDbContext CreateDbContext() => create();

    public Task<PlannerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(create());
}
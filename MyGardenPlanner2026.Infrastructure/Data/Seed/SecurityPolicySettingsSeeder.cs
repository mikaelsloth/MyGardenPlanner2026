namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Generisk, idempotent seeder til ISingletonSettings-entities. Bevidst generisk —
/// modsat de øvrige seedere (én pr. entity-type) — da de 5 policy-settings-typer alle
/// følger nøjagtig samme "singleton-række"-mønster og kun adskiller sig ved deres
/// default-værdier (leveret via createDefault, bygget fra IOptions&lt;T&gt; ved DI-opsætning).
/// </summary>
public sealed class SecurityPolicySettingsSeeder<TEntity>(
    IAdminDbContextFactory contextFactory,
    Func<TEntity> createDefault)
    where TEntity : class, ISingletonSettings
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Set<TEntity>().AnyAsync(cancellationToken))
        {
            return;
        }

        context.Set<TEntity>().Add(createDefault());
        await context.SaveChangesAsync(cancellationToken);
    }
}
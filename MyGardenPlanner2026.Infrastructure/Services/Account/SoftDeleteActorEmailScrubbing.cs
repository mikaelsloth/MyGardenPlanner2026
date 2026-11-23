namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using System.Reflection;

/// <summary>
/// R10.11: nulstiller DeletedByEmail for alle ISoftDelete-entiteter, hvor DeletedByUserId er brugeren.
/// Entiteterne findes via EF-modellen, så nye entiteter dækkes automatisk. adminSchema = true renser
/// kun admin-schema (kræver admin-forbindelsen), false renser resten (dbo). ExecuteUpdate går uden om
/// interceptors og bruges derfor kun til denne målrettede rensning.
/// </summary>
public static class SoftDeleteActorEmailScrubbing
{
    private static readonly MethodInfo ClearForEntityMethod =
        typeof(SoftDeleteActorEmailScrubbing).GetMethod(nameof(ClearForEntityAsync), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static IReadOnlyList<Type> SoftDeleteEntityTypes(IModel model, bool adminSchema)
    {
        ArgumentNullException.ThrowIfNull(model);

        return [.. model.GetEntityTypes()
            .Where(t => typeof(ISoftDelete).IsAssignableFrom(t.ClrType))
            .Where(t => IsAdminSchema(t) == adminSchema)
            .Select(t => t.ClrType)];
    }

    public static async Task<int> ClearAsync(
        PlannerDbContext context, string userId, bool adminSchema, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var total = 0;

        foreach (var clrType in SoftDeleteEntityTypes(context.Model, adminSchema))
        {
            var clear = ClearForEntityMethod.MakeGenericMethod(clrType)
                .CreateDelegate<Func<PlannerDbContext, string, CancellationToken, Task<int>>>();

            total += await clear(context, userId, cancellationToken);
        }

        return total;
    }

    private static bool IsAdminSchema(IReadOnlyEntityType entityType) =>
        string.Equals(entityType.GetSchema(), PlannerDbContext.AdminSchema, StringComparison.Ordinal);

    private static Task<int> ClearForEntityAsync<T>(
        PlannerDbContext context, string userId, CancellationToken cancellationToken)
        where T : class, ISoftDelete =>
        context.Set<T>()
            .IgnoreQueryFilters()
            .Where(e => e.DeletedByUserId == userId && e.DeletedByEmail != null)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.DeletedByEmail, (string?)null), cancellationToken);
}
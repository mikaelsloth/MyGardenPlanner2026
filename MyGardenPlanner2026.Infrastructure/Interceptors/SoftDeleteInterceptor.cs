namespace MyGardenPlanner2026.Infrastructure.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Konverterer sletning (EntityState.Deleted) af ISoftDelete-entities til en opdatering
/// der sætter IsDeleted = true, DeletedAtUtc og DeletedBy, i stedet for en fysisk DELETE.
/// </summary>
public sealed class SoftDeleteInterceptor(ICurrentUserAccessor currentUser) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDeletes(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplySoftDeletes(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void ApplySoftDeletes(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var deletedEntries = context.ChangeTracker.Entries<ISoftDelete>()
            .Where(e => e.State == EntityState.Deleted);

        var user = currentUser.GetCurrent();

        foreach (var entry in deletedEntries)
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = DateTimeOffset.UtcNow;
            entry.Entity.DeletedBy = user.UserEmail ?? user.UserId ?? "system";
        }
    }
}
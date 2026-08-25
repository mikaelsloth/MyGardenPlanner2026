namespace MyGardenPlanner2026.Infrastructure.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Text.Json;

/// <summary>
/// Registrerer Create/Update/Delete på alle entities der implementerer ISoftDelete
/// som append-only rækker i admin.AuditLogs.
///
/// EntityId-timing: bruger EntityEntry.Property(...).IsTemporary til generisk at afgøre,
/// om nøgleværdien allerede er kendt på SavingChanges-tidspunktet:
/// - Entities med klient-genereret Guid-nøgle (ValueGeneratedNever + konstruktør-default,
///   fx Guid.CreateVersion7()) har IsTemporary == false med det samme -> EntityId logges
///   korrekt i samme SaveChanges-kald, INGEN ekstra database-roundtrip.
/// - Entities med database-genereret identity-nøgle (int) har IsTemporary == true indtil
///   INSERT er udført -> falder tilbage til to-trins-opdatering via SavedChanges.
///
/// Skal registreres EFTER SoftDeleteInterceptor i AddInterceptors(...)-listen: en fysisk
/// sletning er på dette tidspunkt allerede konverteret til EntityState.Modified med
/// IsDeleted ændret false -> true, hvilket denne interceptor genkender som AuditAction.Delete.
/// </summary>
public sealed class AuditLoggingInterceptor(ICurrentUserAccessor currentUser) : SaveChangesInterceptor
{
    //private const string PendingEntityIdPlaceholder = "(afventer)";

    //private static readonly ConditionalWeakTable<DbContext, List<PendingCreateLog>> PendingCreateLogsByContext = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        WriteAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        WriteAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    //public override async ValueTask<int> SavedChangesAsync(
    //    SaveChangesCompletedEventData eventData,
    //    int result,
    //    CancellationToken cancellationToken = default)
    //{
    //    await FixUpPendingEntityIdsAsync(eventData.Context, cancellationToken);
    //    return await base.SavedChangesAsync(eventData, result, cancellationToken);
    //}

    //public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    //{
    //    FixUpPendingEntityIdsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
    //    return base.SavedChanges(eventData, result);
    //}

    private void WriteAuditEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var trackedEntries = context.ChangeTracker.Entries<ISoftDelete>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        if (trackedEntries.Count == 0)
        {
            return;
        }

        var user = currentUser.GetCurrent();

        foreach (var entry in trackedEntries)
        {
            var action = DetermineAction(entry);
            //var idIsKnownNow = IsEntityIdKnownNow(entry);

            var log = new AuditLog
            {
                UserId = user.UserId,
                UserEmail = user.UserEmail,
                IpAddress = user.IpAddress,
                Action = action,
                EntityName = entry.Entity.GetType().Name,
                //EntityId = idIsKnownNow ? ResolveEntityId(entry) : PendingEntityIdPlaceholder,
                EntityId = ResolveEntityId(entry),
                OldValues = action == AuditAction.Create ? null : SerializeValues(entry, useOriginal: true),
                NewValues = action == AuditAction.Delete ? null : SerializeValues(entry, useOriginal: false),
                TimestampUtc = DateTimeOffset.UtcNow
            };

            context.Add(log);

            //if (!idIsKnownNow)
            //{
            //    var pendingForThisContext = PendingCreateLogsByContext.GetOrCreateValue(context);
            //    pendingForThisContext.Add(new PendingCreateLog(log, entry));
            //}
        }
    }

    //private static async Task FixUpPendingEntityIdsAsync(DbContext? context, CancellationToken cancellationToken)
    //{
    //    if (context is null)
    //    {
    //        return;
    //    }

    //    if (!PendingCreateLogsByContext.TryGetValue(context, out var pendingForThisContext) || pendingForThisContext.Count == 0)
    //    {
    //        return;
    //    }

    //    foreach (var pending in pendingForThisContext)
    //    {
    //        pending.Log.EntityId = ResolveEntityId(pending.Entry);
    //    }

    //    PendingCreateLogsByContext.Remove(context);

    //    // Kun entities med database-genereret nøgle (fx int identity) rammer denne sti.
    //    // For Guid-nøglede entities (ValueGeneratedNever) udløses dette kald aldrig.
    //    await context.SaveChangesAsync(cancellationToken);
    //}

    //private static bool IsEntityIdKnownNow(EntityEntry<ISoftDelete> entry)
    //{
    //    var keyProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
    //    if (keyProperty is null)
    //    {
    //        return true;
    //    }

    //    return !entry.Property(keyProperty.Name).IsTemporary;
    //}

    private static AuditAction DetermineAction(EntityEntry<ISoftDelete> entry)
    {
        if (entry.State == EntityState.Added)
        {
            return AuditAction.Create;
        }

        var isDeletedProperty = entry.Property(nameof(ISoftDelete.IsDeleted));
        var wasSoftDeleted = isDeletedProperty.IsModified
            && isDeletedProperty.OriginalValue is false
            && isDeletedProperty.CurrentValue is true;

        return wasSoftDeleted ? AuditAction.Delete : AuditAction.Update;
    }

    private static string ResolveEntityId(EntityEntry<ISoftDelete> entry)
    {
        var keyProperty = entry.Metadata.FindPrimaryKey()?.Properties is { Count: > 0 } properties ? properties[0] : null;
        return keyProperty is null ? "ukendt" : entry.Property(keyProperty.Name).CurrentValue?.ToString() ?? "ukendt";
    }

    private static string SerializeValues(EntityEntry<ISoftDelete> entry, bool useOriginal)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            values[property.Metadata.Name] = useOriginal ? property.OriginalValue : property.CurrentValue;
        }

        return JsonSerializer.Serialize(values);
    }

    //private sealed record PendingCreateLog(AuditLog Log, EntityEntry<ISoftDelete> Entry);
}
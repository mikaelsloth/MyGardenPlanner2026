namespace MyGardenPlanner2026.Infrastructure.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Runtime.CompilerServices;
using System.Text.Json;

/// <summary>
/// Registrerer Create/Update/Delete på alle entities der implementerer ISoftDelete
/// som append-only rækker i admin.AuditLogs.
///
/// Skal registreres EFTER SoftDeleteInterceptor i AddInterceptors(...)-listen: en fysisk
/// sletning er på dette tidspunkt allerede konverteret af SoftDeleteInterceptor til
/// EntityState.Modified med IsDeleted ændret false -&gt; true. Denne interceptor genkender
/// det mønster og logger det som AuditAction.Delete i stedet for Update.
///
/// EntityId for nyoprettede entities (auto-increment PK) kan IKKE læses i SavingChanges,
/// da databasen først genererer nøglen under selve INSERT-eksekveringen. Derfor bygges
/// AuditLog-rækken for Create i to trin: den tilføjes i SavingChanges (så den er en del af
/// samme SaveChanges-kald og dermed samme logiske transaktion), men EntityId udfyldes
/// først i SavedChanges, når den rigtige nøgle er kendt — via et ekstra, efterfølgende
/// SaveChanges-kald.
///
/// Klassen er registreret som Singleton i DI, men bruges samtidigt af mange DbContext-
/// instanser (fx samtidige Blazor Server circuits). Pending create-logs kan derfor IKKE
/// gemmes i et almindeligt instansfelt (det ville deles på tværs af samtidige contexts
/// og introducere race conditions). ConditionalWeakTable knytter i stedet pending-listen
/// til den specifikke DbContext-instans, der udløste SavingChanges, og fjernes automatisk
/// hvis contexten garbage-collectes uden at nå SavedChanges.
/// </summary>
public sealed class AuditLoggingInterceptor(ICurrentUserAccessor currentUser) : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, List<PendingCreateLog>> PendingCreateLogsByContext = [];

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

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await FixUpCreateEntityIdsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        FixUpCreateEntityIdsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

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

            var log = new AuditLog
            {
                UserId = user.UserId,
                UserEmail = user.UserEmail,
                IpAddress = user.IpAddress,
                Action = action,
                EntityName = entry.Entity.GetType().Name,
                EntityId = action == AuditAction.Create ? "0" : ResolveEntityId(entry),
                OldValues = action == AuditAction.Create ? null : SerializeValues(entry, useOriginal: true),
                NewValues = action == AuditAction.Delete ? null : SerializeValues(entry, useOriginal: false),
                TimestampUtc = DateTimeOffset.UtcNow
            };

            context.Add(log);

            if (action == AuditAction.Create)
            {
                var pendingForThisContext = PendingCreateLogsByContext.GetOrCreateValue(context);
                pendingForThisContext.Add(new PendingCreateLog(log, entry));
            }
        }
    }

    private static async Task FixUpCreateEntityIdsAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null)
        {
            return;
        }

        if (!PendingCreateLogsByContext.TryGetValue(context, out var pendingForThisContext) || pendingForThisContext.Count == 0)
        {
            return;
        }

        foreach (var pending in pendingForThisContext)
        {
            pending.Log.EntityId = ResolveEntityId(pending.Entry);
        }

        PendingCreateLogsByContext.Remove(context);

        // Persistér de korrigerede EntityId-værdier i en separat, efterfølgende SaveChanges.
        // Dette sker uden for den oprindelige transaktion, men da AuditLogs er append-only
        // og aldrig læses samtidig med skrivning i denne kontekst, er det acceptabelt her.
        await context.SaveChangesAsync(cancellationToken);
    }

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
        var keyProperty = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();
        if (keyProperty is null)
        {
            return "ukendt";
        }

        return entry.Property(keyProperty.Name).CurrentValue?.ToString() ?? "ukendt";
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

    private sealed record PendingCreateLog(AuditLog Log, EntityEntry<ISoftDelete> Entry);
}
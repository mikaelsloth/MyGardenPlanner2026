namespace MyGardenPlanner2026.Infrastructure.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

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
                EntityId = ResolveEntityId(entry),
                OldValues = action == AuditAction.Create ? null : SerializeValues(entry, useOriginal: true),
                NewValues = action == AuditAction.Delete ? null : SerializeValues(entry, useOriginal: false),
                TimestampUtc = DateTimeOffset.UtcNow
            };

            context.Add(log);
        }
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

        return JsonSerializer.Serialize(values, SerializationOptions);
    }
}
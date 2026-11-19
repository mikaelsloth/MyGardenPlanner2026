namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Bygger eksplicitte AuditLog-rækker for livscyklus-handlinger, der bruger ExecuteDelete/ExecuteUpdate
/// og derfor går uden om AuditLoggingInterceptor (kontosletning, gendannelse og oprydning af haver, R10.4).
/// Værdierne må ALDRIG indeholde e-mail, havenavne eller andre persondata: rækkerne bevares i 5 år,
/// mens havens data slettes efter opbevaringsperioden. Kun id'er, udfald, årsag og antal.
/// </summary>
public static class GardenLifecycleAudit
{
    public const string GardenEntity = "Garden";
    public const string MembershipEntity = "GardenMembership";
    public const string AccountDeletionEntity = "AccountDeletion";

    /// <summary>Aktør for handlinger uden bruger (baggrundsjob).</summary>
    public static CurrentUserInfo SystemActor { get; } = new(null, null, null);

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static AuditLog Entry(
        CurrentUserInfo actor,
        DateTimeOffset timestampUtc,
        AuditAction action,
        string entityName,
        string entityId,
        object? oldValues = null,
        object? newValues = null)
    {
        ArgumentNullException.ThrowIfNull(actor);

        return new AuditLog
        {
            UserId = actor.UserId,
            UserEmail = actor.UserEmail,
            IpAddress = actor.IpAddress,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues, SerializationOptions),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues, SerializationOptions),
            TimestampUtc = timestampUtc
        };
    }
}
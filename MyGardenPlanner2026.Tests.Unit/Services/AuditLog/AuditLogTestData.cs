namespace MyGardenPlanner2026.Tests.Unit.Services.AuditLog;

using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Fælles testdata til AuditLog-tests: tomt filter, AuditLog-række med fornuftige
/// standardværdier og seeding via en vilkårlig PlannerDbContext-kilde (SQLite eller SQL Server).
/// </summary>
internal static class AuditLogTestData
{
    /// <summary>Filter uden nogen betingelser. Records er immutable; udvid med "with { ... }".</summary>
    public static readonly AuditLogFilterDto EmptyFilter = new(null, null, null, null, null, null, null);

    public static AuditLog Log(
        string entityName = "SubscriptionTier",
        string entityId = "1",
        string? userId = "user-1",
        string? userEmail = "user1@example.com",
        AuditAction action = AuditAction.Update,
        DateTimeOffset? timestampUtc = null) => new()
        {
            EntityName = entityName,
            EntityId = entityId,
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            TimestampUtc = timestampUtc ?? DateTimeOffset.UtcNow
        };

    public static async Task SeedAsync(
        Func<PlannerDbContext> createContext, params AuditLog[] logs)
    {
        await using var context = createContext();
        await context.AuditLogs.AddRangeAsync(logs);
        await context.SaveChangesAsync();
    }

    /// <summary>Læser alle rækker via GetBatchAsync med keyset-cursor, som eksporten gør.</summary>
    public static async Task<List<AuditLogEntryDto>> ReadAllBatchesAsync(
        IAuditLogQueryService service, AuditLogFilterDto filter, int batchSize,
        CancellationToken cancellationToken)
    {
        var all = new List<AuditLogEntryDto>();
        AuditLogCursor? cursor = null;

        while (true)
        {
            var batch = await service.GetBatchAsync(filter, cursor, batchSize, cancellationToken);
            all.AddRange(batch);

            if (batch.Count < batchSize)
            {
                return all;
            }

            cursor = new AuditLogCursor(batch[^1].TimestampUtc, batch[^1].Id);
        }
    }
}
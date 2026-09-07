namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Rent læse-adgang til admin.AuditLogs. Bruger den begrænsede appbruger
/// (IDbContextFactory&lt;PlannerDbContext&gt;, ikke IAdminDbContextFactory) —
/// denne service skriver aldrig, og appbrugeren har allerede SELECT på
/// admin-schema (se IAdminDbContextFactory-dokumentationen).
///
/// Provider-bevidst: SQL Server får fuld server-side Where/OrderBy/Skip/Take.
/// SQLite (unit-tests) materialiserer FØRST via ToListAsync, og filtrerer/
/// sorterer/paginerer derefter med rene Enumerable-operatorer på List&lt;AuditLog&gt;
/// — samme mønster som RoleElevationExpirySweepService/JitElevationService.
/// Bruger bevidst IKKE List.AsQueryable(): det konverterer til IQueryable,
/// hvorved efterfølgende LINQ-kald resolver til Queryable-operatorer i stedet
/// for Enumerable — hvilket udløser "provider doesn't implement IAsyncQueryProvider".
/// </summary>
public sealed class AuditLogQueryService(
    IDbContextFactory<PlannerDbContext> contextFactory) : IAuditLogQueryService
{
    public async Task<AuditLogQueryResultDto> SearchAsync(
        AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ValidatePaging(filter);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        return context.Database.IsSqlServer()
            ? await SearchOnSqlServerAsync(context, filter, cancellationToken)
            : await SearchInMemoryAsync(context, filter, cancellationToken);
    }

    public async Task<int> CountAsync(
        AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        if (context.Database.IsSqlServer())
        {
            return await ApplyFilter(context.AuditLogs, filter).CountAsync(cancellationToken);
        }

        var all = await context.AuditLogs.ToListAsync(cancellationToken);
        return ApplyFilter(all, filter).Count();
    }

    public async Task<IReadOnlyList<string>> GetDistinctEntityNamesAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Ingen provider-forgrening nødvendig: Distinct/OrderBy på en string-kolonne
        // rammer ikke SQLite-begrænsningen på DateTimeOffset (jf. memory-noter).
        return await context.AuditLogs
            .Select(a => a.EntityName)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);
    }

    private static async Task<AuditLogQueryResultDto> SearchOnSqlServerAsync(
        PlannerDbContext context, AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var query = ApplyFilter(context.AuditLogs, filter);
        var totalCount = await query.CountAsync(cancellationToken);

        query = filter.SortDescending
            ? query.OrderByDescending(a => a.TimestampUtc)
            : query.OrderBy(a => a.TimestampUtc);

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);

        return new AuditLogQueryResultDto(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    private static async Task<AuditLogQueryResultDto> SearchInMemoryAsync(
        PlannerDbContext context, AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var all = await context.AuditLogs.ToListAsync(cancellationToken);

        var filtered = ApplyFilter(all, filter).ToList();
        var totalCount = filtered.Count;

        IEnumerable<AuditLog> sorted = filter.SortDescending
            ? filtered.OrderByDescending(a => a.TimestampUtc)
            : filtered.OrderBy(a => a.TimestampUtc);

        var items = sorted
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(ToDto)
            .ToList();

        return new AuditLogQueryResultDto(items, totalCount, filter.PageNumber, filter.PageSize);
    }

    /// <summary>SQL Server-sti — oversættes af EF Core til SQL (Expression-baseret).</summary>
    private static IQueryable<AuditLog> ApplyFilter(IQueryable<AuditLog> query, AuditLogFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            query = query.Where(a => a.EntityName == filter.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityId))
        {
            query = query.Where(a => a.EntityId == filter.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            query = query.Where(a => a.UserId == filter.UserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserEmail))
        {
            query = query.Where(a => a.UserEmail == filter.UserEmail);
        }

        if (filter.Action is { } action)
        {
            query = query.Where(a => a.Action == action);
        }

        if (filter.FromUtc is { } from)
        {
            query = query.Where(a => a.TimestampUtc >= from);
        }

        if (filter.ToUtc is { } to)
        {
            query = query.Where(a => a.TimestampUtc <= to);
        }

        return query;
    }

    /// <summary>SQLite/in-memory-sti — rene LINQ-to-Objects Enumerable-operatorer (Func-baseret).</summary>
    private static IEnumerable<AuditLog> ApplyFilter(IEnumerable<AuditLog> source, AuditLogFilterDto filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            source = source.Where(a => a.EntityName == filter.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(filter.EntityId))
        {
            source = source.Where(a => a.EntityId == filter.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserId))
        {
            source = source.Where(a => a.UserId == filter.UserId);
        }

        if (!string.IsNullOrWhiteSpace(filter.UserEmail))
        {
            source = source.Where(a => a.UserEmail == filter.UserEmail);
        }

        if (filter.Action is { } action)
        {
            source = source.Where(a => a.Action == action);
        }

        if (filter.FromUtc is { } from)
        {
            source = source.Where(a => a.TimestampUtc >= from);
        }

        if (filter.ToUtc is { } to)
        {
            source = source.Where(a => a.TimestampUtc <= to);
        }

        return source;
    }

    private static void ValidatePaging(AuditLogFilterDto filter)
    {
        if (filter.PageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(filter), "PageNumber skal være mindst 1.");
        }

        if (filter.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(filter), "PageSize skal være mindst 1.");
        }
    }

    private static AuditLogEntryDto ToDto(AuditLog log) => new(
        log.Id, log.UserId, log.UserEmail, log.IpAddress, log.Action,
        log.EntityName, log.EntityId, log.OldValues, log.NewValues, log.TimestampUtc);
}
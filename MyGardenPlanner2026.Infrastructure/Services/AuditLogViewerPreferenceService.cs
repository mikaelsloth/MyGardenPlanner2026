namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Læser/skriver via den begrænsede appbruger (IDbContextFactory&lt;PlannerDbContext&gt;),
/// ikke IAdminDbContextFactory — dette er almindelig brugerdata, ikke en beskyttet
/// §3.2-entity. JsonStringEnumConverter bruges for læsbar/robust serialisering af
/// AuditAction i det gemte filter (samme konvention som AuditLoggingInterceptor).
/// </summary>
public sealed class AuditLogViewerPreferenceService(
    IDbContextFactory<PlannerDbContext> contextFactory) : IAuditLogViewerPreferenceService
{
    private const int DefaultPageSize = 25;

    private static readonly JsonSerializerOptions SerializationOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<AuditLogViewerPreferenceDto> GetAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.AuditLogViewerPreferences
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        return entity is null
            ? new AuditLogViewerPreferenceDto(DefaultPageSize, null)
            : new AuditLogViewerPreferenceDto(entity.PageSize, DeserializeFilter(entity.LastFilterJson));
    }

    public async Task SaveAsync(
        string userId, AuditLogViewerPreferenceDto preference, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(preference);

        if (preference.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(preference), "PageSize skal være mindst 1.");
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var entity = await context.AuditLogViewerPreferences
            .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        var filterJson = SerializeFilter(preference.LastFilter);

        if (entity is null)
        {
            entity = new AuditLogViewerPreference
            {
                UserId = userId,
                PageSize = preference.PageSize,
                LastFilterJson = filterJson
            };
            await context.AuditLogViewerPreferences.AddAsync(entity, CancellationToken.None);
        }
        else
        {
            entity.PageSize = preference.PageSize;
            entity.LastFilterJson = filterJson;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static string? SerializeFilter(AuditLogFilterDto? filter) =>
        filter is null ? null : JsonSerializer.Serialize(filter, SerializationOptions);

    private static AuditLogFilterDto? DeserializeFilter(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<AuditLogFilterDto>(json, SerializationOptions);
}
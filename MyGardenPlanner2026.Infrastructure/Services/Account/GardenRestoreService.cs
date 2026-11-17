namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Standardimplementering af IGardenRestoreService. Rækkerne er soft-slettet af
/// AccountDeletionService (R10.1b) og nøgles på ejerens gamle UserId; ved gendannelse flyttes
/// medlemskab og entitlement til det nye UserId. Bruger ExecuteUpdate (går uden om interceptors),
/// samme mønster som AccountDeletionService. Tidsvindue-filtrering sker i hukommelsen
/// (DateTimeOffset-sammenligning kan ikke oversættes til SQL på SQLite).
/// </summary>
public sealed partial class GardenRestoreService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    TimeProvider timeProvider,
    ILogger<GardenRestoreService> logger) : IGardenRestoreService
{
    private const string NotAvailableMessage = "Gendannelsen findes ikke eller er ikke tilgængelig for din e-mail.";

    [LoggerMessage(EventId = 1163, Level = LogLevel.Information, Message = "Have '{GardenId}' gendannet for bruger '{UserId}' (tidligere bruger '{FormerUserId}').")]
    static partial void GardenRestored(ILogger logger, Guid GardenId, string UserId, string FormerUserId);

    [LoggerMessage(EventId = 1164, Level = LogLevel.Information, Message = "Gendannelse '{PendingRestoreId}' afvist for bruger '{UserId}': {Reason}.")]
    static partial void GardenRestoreRejected(ILogger logger, Guid PendingRestoreId, string UserId, string Reason);

    public async Task<IReadOnlyList<RestorableGardenDto>> GetRestorableGardensAsync(
        string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var emailHash = RestoreEmailHasher.Hash(email);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var pending = await context.PendingGardenRestores
            .AsNoTracking()
            .Where(p => p.OwnerEmailHash == emailHash)
            .ToListAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var active = pending.Where(p => p.ExpiresUtc >= now).ToList();
        if (active.Count == 0)
        {
            return [];
        }

        var gardenIds = active.Select(p => p.GardenId).ToList();
        var gardens = await context.Gardens
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(g => gardenIds.Contains(g.Id))
            .ToListAsync(cancellationToken);

        return
        [
            .. active
                .Join(gardens, p => p.GardenId, g => g.Id,
                    (p, g) => new RestorableGardenDto(p.Id, g.Id, g.Name, p.DeletedAtUtc, p.ExpiresUtc))
                .OrderBy(r => r.Name, StringComparer.CurrentCulture)
        ];
    }

    public async Task<Guid> RestoreGardenAsync(
        Guid pendingRestoreId, string userId, string email, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var emailHash = RestoreEmailHasher.Hash(email);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var pending = await context.PendingGardenRestores
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == pendingRestoreId && p.OwnerEmailHash == emailHash, cancellationToken);

        if (pending is null)
        {
            GardenRestoreRejected(logger, pendingRestoreId, userId, "ukendt gendannelse eller anden e-mail");
            throw new InvalidOperationException(NotAvailableMessage);
        }

        if (pending.ExpiresUtc < timeProvider.GetUtcNow())
        {
            GardenRestoreRejected(logger, pendingRestoreId, userId, "gendannelsesperioden er udløbet");
            throw new InvalidOperationException("Gendannelsesperioden er udløbet.");
        }

        var gardenId = pending.GardenId;
        var formerUserId = pending.FormerUserId;
        var restoreId = pending.Id;

        DateTimeOffset? noDeletedAt = null;
        string? noDeletedBy = null;

        var gardens = await context.Gardens.IgnoreQueryFilters()
            .Where(g => g.Id == gardenId && g.IsDeleted)
            .ExecuteUpdateAsync(
                s => s.SetProperty(g => g.IsDeleted, false)
                      .SetProperty(g => g.DeletedAtUtc, noDeletedAt)
                      .SetProperty(g => g.DeletedBy, noDeletedBy),
                cancellationToken);

        var memberships = await context.GardenMemberships.IgnoreQueryFilters()
            .Where(m => m.GardenId == gardenId && m.UserId == formerUserId && m.IsDeleted)
            .ExecuteUpdateAsync(
                s => s.SetProperty(m => m.UserId, userId)
                      .SetProperty(m => m.IsDeleted, false)
                      .SetProperty(m => m.DeletedAtUtc, noDeletedAt)
                      .SetProperty(m => m.DeletedBy, noDeletedBy),
                cancellationToken);

        if (gardens != 1 || memberships != 1)
        {
            // Integritetsfejl: transaktionen rulles tilbage ved dispose.
            GardenRestoreRejected(logger, pendingRestoreId, userId, "havens eller ejerens medlemskabs data mangler");
            throw new InvalidOperationException("Haven kan ikke gendannes, fordi data mangler.");
        }

        // Entitlementet gendannes uændret (også hvis ValidToUtc er passeret). Findes ingen, er det ok.
        await context.UserEntitlements.IgnoreQueryFilters()
            .Where(e => e.GardenId == gardenId && e.UserId == formerUserId && e.IsDeleted)
            .ExecuteUpdateAsync(
                s => s.SetProperty(e => e.UserId, userId)
                      .SetProperty(e => e.IsDeleted, false)
                      .SetProperty(e => e.DeletedAtUtc, noDeletedAt)
                      .SetProperty(e => e.DeletedBy, noDeletedBy),
                cancellationToken);

        await context.PendingGardenRestores
            .Where(p => p.Id == restoreId)
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        GardenRestored(logger, gardenId, userId, formerUserId);

        return gardenId;
    }
}
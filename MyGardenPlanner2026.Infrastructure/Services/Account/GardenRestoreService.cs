namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Standardimplementering af IGardenRestoreService. Tidsvindue-filtrering sker i hukommelsen
/// (DateTimeOffset-sammenligning kan ikke oversættes til SQL på SQLite). Havens tidligere ejerniveau
/// læses fra ejerens soft-slettede medlemskab (bevaret af AccountDeletionService). En gendannelsesrække
/// uden have eller uden ejerens medlemskab udelades.
/// </summary>
public sealed class GardenRestoreService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    TimeProvider timeProvider) : IGardenRestoreService
{
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

        var deletedMemberships = await context.GardenMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => gardenIds.Contains(m.GardenId) && m.IsDeleted)
            .ToListAsync(cancellationToken);

        return
        [
            .. active
                .Select(p => (
                    Ticket: p,
                    Garden: gardens.FirstOrDefault(g => g.Id == p.GardenId),
                    Owner: deletedMemberships.FirstOrDefault(m => m.GardenId == p.GardenId && m.UserId == p.FormerUserId)))
                .Where(x => x.Garden is not null && x.Owner is not null)
                .Select(x => new RestorableGardenDto(
                    x.Ticket.Id, x.Garden!.Id, x.Garden.Name, x.Ticket.DeletedAtUtc, x.Ticket.ExpiresUtc,
                    x.Owner!.Layer, x.Owner.Category))
                .OrderBy(r => r.Name, StringComparer.CurrentCulture)
        ];
    }
}
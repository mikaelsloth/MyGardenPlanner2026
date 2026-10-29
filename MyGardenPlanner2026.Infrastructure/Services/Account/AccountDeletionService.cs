namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Standardimplementering af IAccountDeletionService. Garden-tabellerne har ingen FK til
/// AspNetUsers (kun UserId som streng), så brugersletning kaskaderer ikke af sig selv.
/// Sletning er fysisk (IgnoreQueryFilters + ExecuteDelete), så personhenførbare data forsvinder;
/// ExecuteDelete går uden om interceptors og skriver derfor ingen AuditLog-rækker (loggen
/// 'AccountDataDeleted' dokumenterer sletningen). AuditLogs og RoleElevationRequests røres ikke.
/// re-auth-fejlforsøg ligger i admin-schema og slettes via IReAuthFailureTracker.
/// </summary>
public sealed partial class AccountDeletionService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    IReAuthFailureTracker reAuthFailureTracker,
    ILogger<AccountDeletionService> logger) : IAccountDeletionService
{
    [LoggerMessage(EventId = 1081, Level = LogLevel.Information, Message = "Account data deleted for user '{UserId}': {Gardens} gardens, {Memberships} memberships, {Entitlements} entitlements, {Drafts} checkout drafts, {InvitationsDeleted} invitations deleted, {InvitationsRevoked} invitations revoked.")]
    static partial void AccountDataDeleted(ILogger logger, string UserId, int Gardens, int Memberships, int Entitlements, int Drafts, int InvitationsDeleted, int InvitationsRevoked);

    [LoggerMessage(EventId = 1082, Level = LogLevel.Warning, Message = "Account deletion for user '{UserId}' blocked: sole owner of {GardenCount} garden(s) with other members.")]
    static partial void AccountDeletionBlocked(ILogger logger, string UserId, int GardenCount);

    public async Task<AccountDeletionResult> DeleteUserDataAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var plan = await PlanAsync(context, userId, cancellationToken);
        if (plan.BlockingGardenNames.Count > 0)
        {
            AccountDeletionBlocked(logger, userId, plan.BlockingGardenNames.Count);
            return AccountDeletionResult.Blocked(plan.BlockingGardenNames);
        }

        var gardenIdsToDelete = plan.GardenIdsToDelete;

        // Eneste medlem: haven slettes (medlemskaber, entitlements og invitationer kaskaderer i databasen).
        var gardens = gardenIdsToDelete.Count == 0
            ? 0
            : await context.Gardens.IgnoreQueryFilters()
                .Where(g => gardenIdsToDelete.Contains(g.Id))
                .ExecuteDeleteAsync(cancellationToken);

        var memberships = await context.GardenMemberships.IgnoreQueryFilters()
            .Where(m => m.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        var entitlements = await context.UserEntitlements.IgnoreQueryFilters()
            .Where(e => e.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        var drafts = await context.CheckoutDrafts
            .Where(d => d.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // D11: accepterede invitationer slettes, ubrugte ugyldiggøres.
        var invitationsDeleted = await context.GardenInvitations.IgnoreQueryFilters()
            .Where(i => i.InvitedByUserId == userId && i.IsAccepted)
            .ExecuteDeleteAsync(cancellationToken);

        var invitationsRevoked = await context.GardenInvitations.IgnoreQueryFilters()
            .Where(i => i.InvitedByUserId == userId && !i.IsAccepted && !i.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsRevoked, true), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await reAuthFailureTracker.ClearFailuresAsync(userId, cancellationToken);

        AccountDataDeleted(logger, userId, gardens, memberships, entitlements, drafts, invitationsDeleted, invitationsRevoked);
        return AccountDeletionResult.Completed();
    }

    private static async Task<DeletionPlan> PlanAsync(
        PlannerDbContext context, string userId, CancellationToken cancellationToken)
    {
        // Query-filteret udelader soft-slettede medlemskaber og haver: kun aktive indgår i planen.
        var gardenIds = await context.GardenMemberships
            .Where(m => m.UserId == userId)
            .Select(m => m.GardenId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var gardens = (await context.Gardens
            .Where(g => gardenIds.Contains(g.Id))
            .ToListAsync(cancellationToken))
            .OrderBy(g => g.Name, StringComparer.CurrentCulture)
            .ToList();

        var activeGardenIds = gardens.Select(g => g.Id).ToList();
        var membersByGarden = (await context.GardenMemberships
            .Where(m => activeGardenIds.Contains(m.GardenId))
            .ToListAsync(cancellationToken))
            .ToLookup(m => m.GardenId);

        var toDelete = new List<Guid>();
        var blocking = new List<string>();

        foreach (var garden in gardens)
        {
            var members = membersByGarden[garden.Id].ToList();
            var others = members.Where(m => m.UserId != userId).ToList();

            if (others.Count == 0)
            {
                toDelete.Add(garden.Id);
            }
            else if (members.Any(m => m.UserId == userId && m.IsOwner) && !others.Any(m => m.IsOwner))
            {
                blocking.Add(garden.Name);
            }
        }

        return new DeletionPlan(toDelete, blocking);
    }

    private sealed record DeletionPlan(List<Guid> GardenIdsToDelete, List<string> BlockingGardenNames);
}
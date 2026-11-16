namespace MyGardenPlanner2026.Infrastructure.Services.Account;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Standardimplementering af IAccountDeletionService. Garden-tabellerne har ingen FK til
/// AspNetUsers (kun UserId som streng), så brugersletning kaskaderer ikke af sig selv.
/// Sletning er fysisk (IgnoreQueryFilters + ExecuteDelete), så personhenførbare data forsvinder;
/// ExecuteDelete går uden om interceptors og skriver derfor ingen AuditLog-rækker (loggen
/// 'AccountDataDeleted' dokumenterer sletningen). AuditLogs og RoleElevationRequests røres ikke.
/// re-auth-fejlforsøg ligger i admin-schema og slettes via IReAuthFailureTracker.
/// Invitationer sendt til brugerens e-mail slettes også (match på nuværende e-mail, case-insensitivt).
/// R10.1a: er brugeren eneste ejer i en have med andre medlemmer, kan ejerskabet overdrages til et
/// aktivt medlem (hæves til Administrator) i samme transaktion som sletningen.
/// R10.1b: haver, hvor brugeren er eneste medlem, og haver valgt "slet for alle", soft-slettes og
/// bevares i GardenRestoreOptions.RetentionDays (PendingGardenRestore, matchet via e-mail-hash).
/// Ejerens medlemskab/entitlement bevares soft-slettet; andres medlemskaber, entitlements og
/// invitationer slettes fysisk. Uden e-mail kan haven ikke gendannes og slettes fysisk.
/// </summary>
public sealed partial class AccountDeletionService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    IReAuthFailureTracker reAuthFailureTracker,
    IOptionsMonitor<GardenRestoreOptions> restoreOptions,
    TimeProvider timeProvider,
    ILogger<AccountDeletionService> logger) : IAccountDeletionService
{
    private static readonly IReadOnlyDictionary<Guid, GardenOwnershipResolution> NoResolutions =
        new Dictionary<Guid, GardenOwnershipResolution>();

    [LoggerMessage(EventId = 1081, Level = LogLevel.Information, Message = "Account data deleted for user '{UserId}': {Gardens} gardens, {Memberships} memberships, {Entitlements} entitlements, {Drafts} checkout drafts, {InvitationsDeleted} invitations sent by the user deleted, {InvitationsToEmailDeleted} invitations to the user's e-mail deleted, {InvitationsRevoked} pending invitations revoked.")]
    static partial void AccountDataDeleted(ILogger logger, string UserId, int Gardens, int Memberships, int Entitlements, int Drafts, int InvitationsDeleted, int InvitationsToEmailDeleted, int InvitationsRevoked);

    [LoggerMessage(EventId = 1082, Level = LogLevel.Warning, Message = "Account deletion for user '{UserId}' blocked: sole owner of {GardenCount} garden(s) with other members.")]
    static partial void AccountDeletionBlocked(ILogger logger, string UserId, int GardenCount);

    [LoggerMessage(EventId = 1160, Level = LogLevel.Information, Message = "Ownership of garden '{GardenId}' transferred from deleted user '{UserId}' to user '{NewOwnerUserId}' (promoted to Administrator).")]
    static partial void GardenOwnershipTransferred(ILogger logger, Guid GardenId, string UserId, string NewOwnerUserId);

    [LoggerMessage(EventId = 1161, Level = LogLevel.Information, Message = "Ownership resolution for garden '{GardenId}' rejected for user '{UserId}': {Reason}.")]
    static partial void GardenOwnershipResolutionRejected(ILogger logger, Guid GardenId, string UserId, string Reason);

    [LoggerMessage(EventId = 1162, Level = LogLevel.Information, Message = "Account deletion for user '{UserId}': {GardenCount} garden(s) soft-deleted and retained for restore until {ExpiresUtc}.")]
    static partial void GardensRetainedForRestore(ILogger logger, string UserId, int GardenCount, DateTimeOffset ExpiresUtc);

    public async Task<IReadOnlyList<BlockingGardenDto>> GetBlockingGardensAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var plan = await PlanAsync(context, userId, cancellationToken);
        return plan.BlockingGardens;
    }

    public Task<AccountDeletionResult> DeleteUserDataAsync(
        string userId, string? email, CancellationToken cancellationToken = default) =>
        DeleteUserDataAsync(userId, email, NoResolutions, cancellationToken);

    public async Task<AccountDeletionResult> DeleteUserDataAsync(
        string userId, string? email, IReadOnlyDictionary<Guid, GardenOwnershipResolution> resolutions,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(resolutions);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var plan = await PlanAsync(context, userId, cancellationToken);

        var transfers = new List<(Guid GardenId, string NewOwnerUserId)>();
        var deleteForAllIds = new List<Guid>();
        var unresolved = new List<BlockingGardenDto>();
        foreach (var blocking in plan.BlockingGardens)
        {
            if (!resolutions.TryGetValue(blocking.GardenId, out var resolution))
            {
                unresolved.Add(blocking);
            }
            else if (resolution.Kind == GardenResolutionKind.DeleteForAll)
            {
                deleteForAllIds.Add(blocking.GardenId);
            }
            else if (!blocking.Candidates.Any(c => c.UserId == resolution.NewOwnerUserId))
            {
                GardenOwnershipResolutionRejected(logger, blocking.GardenId, userId, "recipient is not an active member of the garden");
                unresolved.Add(blocking);
            }
            else
            {
                transfers.Add((blocking.GardenId, resolution.NewOwnerUserId!));
            }
        }

        if (unresolved.Count > 0)
        {
            AccountDeletionBlocked(logger, userId, unresolved.Count);
            return AccountDeletionResult.Blocked(unresolved);
        }

        // R10.1a: ejerskabet overdrages før brugerens egne medlemskaber slettes (samme transaktion).
        foreach (var (gardenId, newOwnerUserId) in transfers)
        {
            var updated = await context.GardenMemberships
                .Where(m => m.GardenId == gardenId && m.UserId == newOwnerUserId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(m => m.IsOwner, true)
                          .SetProperty(m => m.Category, AccessCategory.Administrator),
                    cancellationToken);

            if (updated != 1)
            {
                // Modtageren er forsvundet siden planen blev lagt: transaktionen rulles tilbage ved dispose.
                GardenOwnershipResolutionRejected(logger, gardenId, userId, "recipient is no longer an active member of the garden");
                AccountDeletionBlocked(logger, userId, 1);
                return AccountDeletionResult.Blocked([.. plan.BlockingGardens.Where(g => g.GardenId == gardenId)]);
            }
        }

        // R10.1b: sole-member-haver og "slet for alle" bevares soft-slettet, hvis e-mailen kendes.
        var now = timeProvider.GetUtcNow();
        var expiresUtc = now.AddDays(Math.Max(1, restoreOptions.CurrentValue.RetentionDays));
        var emailHash = string.IsNullOrWhiteSpace(email) ? null : RestoreEmailHasher.Hash(email);

        List<Guid> wholeGardenIds = [.. plan.GardenIdsToDelete, .. deleteForAllIds];
        List<Guid> retainedIds = emailHash is null ? [] : wholeGardenIds;
        List<Guid> purgedIds = emailHash is null ? wholeGardenIds : [];

        // Uden e-mail: haven slettes fysisk (medlemskaber, entitlements og invitationer kaskaderer i databasen).
        var gardens = purgedIds.Count == 0
            ? 0
            : await context.Gardens.IgnoreQueryFilters()
                .Where(g => purgedIds.Contains(g.Id))
                .ExecuteDeleteAsync(cancellationToken);

        var retainedOthersMemberships = 0;
        var retainedOthersEntitlements = 0;
        var retainedInvitations = 0;
        if (retainedIds.Count > 0)
        {
            // Andre medlemmers adgang og invitationer til haven slettes fysisk.
            retainedOthersMemberships = await context.GardenMemberships.IgnoreQueryFilters()
                .Where(m => retainedIds.Contains(m.GardenId) && m.UserId != userId)
                .ExecuteDeleteAsync(cancellationToken);

            retainedOthersEntitlements = await context.UserEntitlements.IgnoreQueryFilters()
                .Where(e => retainedIds.Contains(e.GardenId) && e.UserId != userId)
                .ExecuteDeleteAsync(cancellationToken);

            retainedInvitations = await context.GardenInvitations.IgnoreQueryFilters()
                .Where(i => retainedIds.Contains(i.GardenId))
                .ExecuteDeleteAsync(cancellationToken);

            // Ejerens rækker og haven soft-slettes og bevares til gendannelse/udløb.
            DateTimeOffset? deletedAt = now;

            await context.GardenMemberships
                .Where(m => retainedIds.Contains(m.GardenId) && m.UserId == userId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(m => m.IsDeleted, true)
                          .SetProperty(m => m.DeletedAtUtc, deletedAt)
                          .SetProperty(m => m.DeletedBy, userId),
                    cancellationToken);

            await context.UserEntitlements
                .Where(e => retainedIds.Contains(e.GardenId) && e.UserId == userId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(e => e.IsDeleted, true)
                          .SetProperty(e => e.DeletedAtUtc, deletedAt)
                          .SetProperty(e => e.DeletedBy, userId),
                    cancellationToken);

            await context.Gardens
                .Where(g => retainedIds.Contains(g.Id))
                .ExecuteUpdateAsync(
                    s => s.SetProperty(g => g.IsDeleted, true)
                          .SetProperty(g => g.DeletedAtUtc, deletedAt)
                          .SetProperty(g => g.DeletedBy, userId),
                    cancellationToken);

            foreach (var gardenId in retainedIds)
            {
                context.PendingGardenRestores.Add(new PendingGardenRestore
                {
                    GardenId = gardenId,
                    OwnerEmailHash = emailHash!,
                    FormerUserId = userId,
                    DeletedAtUtc = now,
                    ExpiresUtc = expiresUtc
                });
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        var memberships = await context.GardenMemberships.IgnoreQueryFilters()
            .Where(m => m.UserId == userId && !retainedIds.Contains(m.GardenId))
            .ExecuteDeleteAsync(cancellationToken);

        var entitlements = await context.UserEntitlements.IgnoreQueryFilters()
            .Where(e => e.UserId == userId && !retainedIds.Contains(e.GardenId))
            .ExecuteDeleteAsync(cancellationToken);

        var drafts = await context.CheckoutDrafts
            .Where(d => d.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        // D11/D14: invitationer brugeren har sendt og som er accepteret eller tilbagekaldt, slettes.
        var invitationsDeleted = await context.GardenInvitations.IgnoreQueryFilters()
            .Where(i => i.InvitedByUserId == userId && (i.IsAccepted || i.IsRevoked))
            .ExecuteDeleteAsync(cancellationToken);

        // D14: invitationer sendt TIL brugerens e-mail (persondata) slettes uanset status.
        var invitationsToEmailDeleted = 0;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons -- see https://learn.microsoft.com/da-dk/ef/core/miscellaneous/collations-and-case-sensitivity#translation-of-built-in-net-string-operations
            invitationsToEmailDeleted = await context.GardenInvitations.IgnoreQueryFilters()
                .Where(i => i.Email.Trim().ToLower() == normalizedEmail)
                .ExecuteDeleteAsync(cancellationToken);
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        }

        // D11: brugerens øvrige (ubrugte) invitationer ugyldiggøres.
        var invitationsRevoked = await context.GardenInvitations.IgnoreQueryFilters()
            .Where(i => i.InvitedByUserId == userId && !i.IsAccepted && !i.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsRevoked, true), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await reAuthFailureTracker.ClearFailuresAsync(userId, cancellationToken);

        foreach (var (gardenId, newOwnerUserId) in transfers)
        {
            GardenOwnershipTransferred(logger, gardenId, userId, newOwnerUserId);
        }

        if (retainedIds.Count > 0)
        {
            GardensRetainedForRestore(logger, userId, retainedIds.Count, expiresUtc);
        }

        AccountDataDeleted(
            logger, userId, gardens, memberships + retainedOthersMemberships, entitlements + retainedOthersEntitlements,
            drafts, invitationsDeleted + retainedInvitations, invitationsToEmailDeleted, invitationsRevoked);
        return AccountDeletionResult.Completed();
    }

    private static async Task<DeletionPlan> PlanAsync(
        PlannerDbContext context, string userId, CancellationToken cancellationToken)
    {
        // Query-filteret udelader soft-slettede medlemskaber og haver: kun aktive indgår i planen.
        var gardenIds = await context.GardenMemberships
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Select(m => m.GardenId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var gardens = (await context.Gardens
            .AsNoTracking()
            .Where(g => gardenIds.Contains(g.Id))
            .ToListAsync(cancellationToken))
            .OrderBy(g => g.Name, StringComparer.CurrentCulture)
            .ToList();

        var activeGardenIds = gardens.Select(g => g.Id).ToList();
        var membersByGarden = (await context.GardenMemberships
            .AsNoTracking()
            .Where(m => activeGardenIds.Contains(m.GardenId))
            .ToListAsync(cancellationToken))
            .ToLookup(m => m.GardenId);

        var toDelete = new List<Guid>();
        var blocking = new List<BlockingGardenDto>();

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
                // Sorteres i hukommelsen (DateTimeOffset ORDER BY understøttes ikke i SQLite).
                var candidates = others
                    .OrderByDescending(m => m.Category)
                    .ThenBy(m => m.JoinedAtUtc)
                    .ThenBy(m => m.UserId, StringComparer.Ordinal)
                    .Select(m => new GardenTransferCandidateDto(m.UserId, m.Layer, m.Category))
                    .ToList();

                blocking.Add(new BlockingGardenDto(garden.Id, garden.Name, candidates));
            }
        }

        return new DeletionPlan(toDelete, blocking);
    }

    private sealed record DeletionPlan(List<Guid> GardenIdsToDelete, List<BlockingGardenDto> BlockingGardens);
}
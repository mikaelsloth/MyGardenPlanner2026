namespace MyGardenPlanner2026.Infrastructure.Services.Onboarding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;

/// <summary>
/// Standardimplementering af IOnboardingService. Skriver via den almindelige
/// (begrænsede) IDbContextFactory&lt;PlannerDbContext&gt; — Gardens-entiteterne ligger i
/// dbo-schema, ikke admin-schema.
///
/// Gratis invitation (HavePlanner2026 - Adgang og abonnement.md, §2): 1 slot pr. aktivt
/// (ikke-trial) UserEntitlement på PRÆCIS samme Layer og Category som entitlementet selv.
/// Layer/Category-loft (Adgangsrettigheder.md): en invitation må aldrig give den
/// inviterede bedre rettigheder end afsenderen selv har.
/// </summary>
public sealed partial class OnboardingService(
    IDbContextFactory<PlannerDbContext> contextFactory,
    IInvitationTokenService tokenService,
    TimeProvider timeProvider,
    ILogger<OnboardingService> logger) : IOnboardingService
{
    private static readonly TimeSpan TrialDuration = TimeSpan.FromDays(30);
    private static readonly TimeSpan CheckoutDraftLifetime = TimeSpan.FromMinutes(30);

    [LoggerMessage(EventId = 1100, Level = LogLevel.Information, Message = "Sandkasse-have '{GardenId}' oprettet for bruger '{UserId}'.")]
    static partial void SandboxGardenCreated(ILogger logger, Guid GardenId, string UserId);

    [LoggerMessage(EventId = 1101, Level = LogLevel.Information, Message = "Betalt have '{GardenId}' provisioneret for bruger '{UserId}' (Layer={Layer}, Category={Category}).")]
    static partial void PaidGardenProvisioned(ILogger logger, Guid GardenId, string UserId, GardenAccessLevel Layer, AccessCategory Category);

    [LoggerMessage(EventId = 1102, Level = LogLevel.Information, Message = "Invitation '{InvitationId}' oprettet af bruger '{UserId}' til '{Email}' (IsFreeSlot={IsFreeSlot}).")]
    static partial void InvitationCreated(ILogger logger, Guid InvitationId, string UserId, string Email, bool IsFreeSlot);

    [LoggerMessage(EventId = 1103, Level = LogLevel.Information, Message = "Oprettelse af invitation afvist for bruger '{UserId}': {Reason}")]
    static partial void InvitationCreationRejected(ILogger logger, string UserId, string Reason);

    [LoggerMessage(EventId = 1104, Level = LogLevel.Information, Message = "Invitation '{InvitationId}' tilbagekaldt af bruger '{UserId}'.")]
    static partial void InvitationRevoked(ILogger logger, Guid InvitationId, string UserId);

    [LoggerMessage(EventId = 1105, Level = LogLevel.Information, Message = "Checkout-draft '{DraftId}' gemt for bruger '{UserId}'.")]
    static partial void CheckoutDraftSaved(ILogger logger, Guid DraftId, string UserId);

    [LoggerMessage(EventId = 1106, Level = LogLevel.Information, Message = "Have '{GardenId}' provisioneret fra checkout-draft '{DraftId}' for bruger '{UserId}'.")]
    static partial void CheckoutDraftProvisioned(ILogger logger, Guid DraftId, Guid GardenId, string UserId);

    [LoggerMessage(EventId = 1107, Level = LogLevel.Information, Message = "Provisionering fra checkout-draft '{DraftId}' afvist: {Reason}")]
    static partial void CheckoutDraftProvisionFailed(ILogger logger, Guid DraftId, string Reason);

    [LoggerMessage(EventId = 1108, Level = LogLevel.Information, Message = "Invitation '{InvitationId}' accepteret af bruger '{UserId}' som medlemskab '{MembershipId}'.")]
    static partial void InvitationAccepted(ILogger logger, Guid InvitationId, string UserId, Guid MembershipId);

    [LoggerMessage(EventId = 1109, Level = LogLevel.Information, Message = "Accept af invitation afvist for bruger '{UserId}': {Reason}")]
    static partial void InvitationAcceptanceRejected(ILogger logger, string UserId, string Reason);

    public async Task<SandboxGardenResultDto> CreateSandboxGardenAsync(
        string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        var garden = new Garden { Name = "Min sandkasse-have" };
        var membership = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = userId,
            IsOwner = true,
            Layer = GardenAccessLevel.HaveArkitekt,
            Category = AccessCategory.Administrator,
            JoinedAtUtc = now
        };
        var entitlement = new UserEntitlement
        {
            UserId = userId,
            GardenId = garden.Id,
            Layer = GardenAccessLevel.HaveArkitekt,
            Category = AccessCategory.Administrator,
            BillingCycle = BillingCycle.Monthly,
            IsTrial = true,
            ValidToUtc = now.Add(TrialDuration)
        };

        await context.Gardens.AddAsync(garden, CancellationToken.None);
        await context.GardenMemberships.AddAsync(membership, CancellationToken.None);
        await context.UserEntitlements.AddAsync(entitlement, CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        SandboxGardenCreated(logger, garden.Id, userId);

        return new SandboxGardenResultDto(garden.Id, membership.Id, entitlement.Id);
    }

    public async Task<PaidGardenProvisionResultDto> ProvisionPaidGardenAsync(
        PaidGardenProvisionRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.GardenName);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        var (garden, membership, entitlement) = await ProvisionPaidGardenCoreAsync(context, request, now, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        PaidGardenProvisioned(logger, garden.Id, request.UserId, request.Layer, request.Category);

        return new PaidGardenProvisionResultDto(garden.Id, membership.Id, entitlement.Id);
    }

    public async Task<Guid> SaveCheckoutDraftAsync(
        SaveCheckoutDraftRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.GardenName);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        var draft = new CheckoutDraft
        {
            UserId = request.UserId,
            GardenName = request.GardenName,
            Description = request.Description,
            Layer = request.Layer,
            Category = request.Category,
            BillingCycle = request.BillingCycle,
            AddOnQuantities = new Dictionary<Guid, int>(request.AddOnQuantities),
            CreatedAtUtc = now,
            ExpiresUtc = now.Add(CheckoutDraftLifetime)
        };

        await context.CheckoutDrafts.AddAsync(draft, CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        CheckoutDraftSaved(logger, draft.Id, request.UserId ?? "ukendt (ikke logget ind endnu)");

        return draft.Id;
    }

    public async Task<CheckoutDraftDto?> GetCheckoutDraftAsync(
        Guid draftId, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var draft = await context.CheckoutDrafts.SingleOrDefaultAsync(d => d.Id == draftId, cancellationToken);

        return draft is null || draft.ExpiresUtc < timeProvider.GetUtcNow() ? null : ToDraftDto(draft);
    }

    public async Task<PaidGardenProvisionResultDto> ProvisionPaidGardenFromDraftAsync(
        Guid draftId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        var draft = await context.CheckoutDrafts.SingleOrDefaultAsync(d => d.Id == draftId, cancellationToken)
            ?? throw new InvalidOperationException("Konfigurationen findes ikke eller er allerede brugt.");

        if (draft.ExpiresUtc < now)
        {
            context.CheckoutDrafts.Remove(draft);
            await context.SaveChangesAsync(cancellationToken);

            CheckoutDraftProvisionFailed(logger, draftId, "udløbet");
            throw new InvalidOperationException("Konfigurationen er udløbet. Start venligst forfra.");
        }

        var request = new PaidGardenProvisionRequestDto(
            userId, draft.GardenName, draft.Description, draft.Layer, draft.Category,
            draft.BillingCycle, draft.AddOnQuantities);

        var (garden, membership, entitlement) = await ProvisionPaidGardenCoreAsync(context, request, now, cancellationToken);

        context.CheckoutDrafts.Remove(draft);
        await context.SaveChangesAsync(cancellationToken);

        CheckoutDraftProvisioned(logger, draftId, garden.Id, userId);

        return new PaidGardenProvisionResultDto(garden.Id, membership.Id, entitlement.Id);
    }

    public async Task<CreateInvitationResultDto> CreateInvitationAsync(
        CreateInvitationRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.InvitedByUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Email);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var requester = await context.GardenMemberships.SingleOrDefaultAsync(
            m => m.GardenId == request.GardenId && m.UserId == request.InvitedByUserId, cancellationToken);

        if (requester is null)
        {
            InvitationCreationRejected(logger, request.InvitedByUserId, "afsenderen er ikke medlem af haven");
            throw new InvalidOperationException("Du er ikke medlem af denne have.");
        }

        if (!IsWithinOwnRights(request.TargetLayer, request.TargetCategory, requester))
        {
            InvitationCreationRejected(logger, request.InvitedByUserId, "TargetLayer/TargetCategory overstiger afsenderens egne rettigheder");
            throw new InvalidOperationException("Du kan ikke invitere til et niveau højere end dine egne rettigheder.");
        }

        var maxAllowedLayer = request.AllowSelfUpgrade ? requester.Layer : request.TargetLayer;
        var maxAllowedCategory = request.AllowSelfUpgrade ? requester.Category : request.TargetCategory;

        var isFreeSlot = false;
        if (request.UseFreeSlot)
        {
            if (request.TargetLayer != requester.Layer || request.TargetCategory != requester.Category)
            {
                InvitationCreationRejected(logger, request.InvitedByUserId, "gratis invitation kræver samme Layer/Category som afsenderens eget abonnement");
                throw new InvalidOperationException(
                    "En gratis invitation skal have samme niveau og kategori som dit eget abonnement.");
            }

            var quota = await CalculateFreeQuotaAsync(context, request.GardenId, request.InvitedByUserId, cancellationToken);
            if (quota.RemainingFreeSlots <= 0)
            {
                InvitationCreationRejected(logger, request.InvitedByUserId, "ingen ledige gratis invitationsslots");
                throw new InvalidOperationException("Du har ingen ledige gratis invitationer tilbage.");
            }

            isFreeSlot = true;
        }

        var token = tokenService.GenerateToken();
        var now = timeProvider.GetUtcNow();

        var invitation = new GardenInvitation
        {
            GardenId = request.GardenId,
            InvitedByUserId = request.InvitedByUserId,
            Email = request.Email,
            TokenHash = token.TokenHash,
            TargetLayer = request.TargetLayer,
            TargetCategory = request.TargetCategory,
            MaxAllowedLayer = maxAllowedLayer,
            MaxAllowedCategory = maxAllowedCategory,
            IsFreeSlot = isFreeSlot,
            AllowSelfUpgrade = request.AllowSelfUpgrade,
            ExpiresUtc = now.Add(request.ValidFor),
            CreatedAtUtc = now
        };

        await context.GardenInvitations.AddAsync(invitation, CancellationToken.None);
        await context.SaveChangesAsync(cancellationToken);

        InvitationCreated(logger, invitation.Id, request.InvitedByUserId, request.Email, isFreeSlot);

        return new CreateInvitationResultDto(invitation.Id, token.RawToken, invitation.ExpiresUtc, isFreeSlot);
    }

    public async Task<InvitationValidationResultDto> ValidateInvitationTokenAsync(
        string rawToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return new InvitationValidationResultDto(false, null, "Intet token angivet.");
        }

        var hash = tokenService.HashToken(rawToken);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var invitation = await context.GardenInvitations.SingleOrDefaultAsync(i => i.TokenHash == hash, cancellationToken);
        return invitation switch
        {
            null => new InvitationValidationResultDto(
                false,
                null,
                "Invitationen findes ikke eller er ugyldig."),

            { IsRevoked: true } => new InvitationValidationResultDto(
                false,
                ToDto(invitation),
                "Invitationen er blevet tilbagekaldt."),

            { IsAccepted: true } => new InvitationValidationResultDto(
                false,
                ToDto(invitation),
                "Invitationen er allerede accepteret."),

            { ExpiresUtc: var expiresUtc } when expiresUtc < timeProvider.GetUtcNow() =>
                new InvitationValidationResultDto(
                    false,
                    ToDto(invitation),
                    "Invitationen er udløbet."),

            _ => new InvitationValidationResultDto(
                true,
                ToDto(invitation),
                null)
        };
    }

    public async Task RevokeInvitationAsync(
        Guid invitationId, string revokedByUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(revokedByUserId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var invitation = await context.GardenInvitations.SingleOrDefaultAsync(i => i.Id == invitationId, cancellationToken)
            ?? throw new InvalidOperationException($"Ingen invitation fundet med Id {invitationId}.");

        if (invitation.IsAccepted)
        {
            throw new InvalidOperationException("En allerede accepteret invitation kan ikke tilbagekaldes.");
        }

        if (invitation.IsRevoked)
        {
            return;
        }

        invitation.IsRevoked = true;
        await context.SaveChangesAsync(cancellationToken);

        InvitationRevoked(logger, invitationId, revokedByUserId);
    }

    public async Task<FreeInvitationQuotaDto> GetFreeInvitationQuotaAsync(
        Guid gardenId, string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await CalculateFreeQuotaAsync(context, gardenId, userId, cancellationToken);
    }

    public async Task<AcceptInvitationResultDto> AcceptInvitationAsync(
        AcceptInvitationRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.RawToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);

        var hash = tokenService.HashToken(request.RawToken);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var now = timeProvider.GetUtcNow();

        var invitation = await context.GardenInvitations.SingleOrDefaultAsync(i => i.TokenHash == hash, cancellationToken)
            ?? throw new InvalidOperationException("Invitationen findes ikke eller er ugyldig.");

        if (invitation.IsRevoked)
        {
            InvitationAcceptanceRejected(logger, request.UserId, "invitationen er tilbagekaldt");
            throw new InvalidOperationException("Invitationen er blevet tilbagekaldt.");
        }

        if (invitation.IsAccepted)
        {
            InvitationAcceptanceRejected(logger, request.UserId, "invitationen er allerede accepteret");
            throw new InvalidOperationException("Invitationen er allerede accepteret.");
        }

        if (invitation.ExpiresUtc < now)
        {
            InvitationAcceptanceRejected(logger, request.UserId, "invitationen er udløbet");
            throw new InvalidOperationException("Invitationen er udløbet.");
        }

        if (!IsWithinInvitationCeiling(request.GrantedLayer, request.GrantedCategory, invitation))
        {
            InvitationAcceptanceRejected(logger, request.UserId, "GrantedLayer/GrantedCategory overstiger invitationens loft");
            throw new InvalidOperationException("Det valgte niveau overstiger invitationens tilladte loft.");
        }

        var alreadyMember = await context.GardenMemberships.AnyAsync(
            m => m.GardenId == invitation.GardenId && m.UserId == request.UserId, cancellationToken);
        if (alreadyMember)
        {
            InvitationAcceptanceRejected(logger, request.UserId, "brugeren er allerede medlem af haven");
            throw new InvalidOperationException("Du er allerede medlem af denne have.");
        }

        var membership = new GardenMembership
        {
            GardenId = invitation.GardenId,
            UserId = request.UserId,
            IsOwner = false,
            Layer = request.GrantedLayer,
            Category = request.GrantedCategory,
            JoinedAtUtc = now
        };
        await context.GardenMemberships.AddAsync(membership, CancellationToken.None);

        Guid? entitlementId = null;
        if (request.UpgradeBillingCycle is { } billingCycle)
        {
            var entitlement = new UserEntitlement
            {
                UserId = request.UserId,
                GardenId = invitation.GardenId,
                Layer = request.GrantedLayer,
                Category = request.GrantedCategory,
                BillingCycle = billingCycle,
                IsTrial = false,
                ValidToUtc = ComputeValidTo(billingCycle, now)
            };

            await ApplyAddOnQuantitiesAsync(context, entitlement, request.AddOnQuantities, cancellationToken);

            await context.UserEntitlements.AddAsync(entitlement, CancellationToken.None);
            entitlementId = entitlement.Id;
        }

        invitation.IsAccepted = true;

        await context.SaveChangesAsync(cancellationToken);

        InvitationAccepted(logger, invitation.Id, request.UserId, membership.Id);

        return new AcceptInvitationResultDto(invitation.GardenId, membership.Id, entitlementId);
    }

    private static async Task<(Garden Garden, GardenMembership Membership, UserEntitlement Entitlement)> ProvisionPaidGardenCoreAsync(
        PlannerDbContext context, PaidGardenProvisionRequestDto request, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var garden = new Garden { Name = request.GardenName, Description = request.Description };
        var membership = new GardenMembership
        {
            GardenId = garden.Id,
            UserId = request.UserId,
            IsOwner = true,
            Layer = request.Layer,
            Category = request.Category,
            JoinedAtUtc = now
        };
        var entitlement = new UserEntitlement
        {
            UserId = request.UserId,
            GardenId = garden.Id,
            Layer = request.Layer,
            Category = request.Category,
            BillingCycle = request.BillingCycle,
            IsTrial = false,
            ValidToUtc = ComputeValidTo(request.BillingCycle, now)
        };

        await ApplyAddOnQuantitiesAsync(context, entitlement, request.AddOnQuantities, cancellationToken);

        await context.Gardens.AddAsync(garden, CancellationToken.None);
        await context.GardenMemberships.AddAsync(membership, CancellationToken.None);
        await context.UserEntitlements.AddAsync(entitlement, CancellationToken.None);

        return (garden, membership, entitlement);
    }

    private async Task<FreeInvitationQuotaDto> CalculateFreeQuotaAsync(
        PlannerDbContext context, Guid gardenId, string userId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        // OBS: ValidToUtc-sammenligningen kan ikke oversættes til SQL på SQLite —
        // entitlements materialiseres derfor først, og tidsvindue-filtreres i
        // hukommelsen bagefter, samme mønster som JitElevationService.HasActiveElevationAsync.
        var candidateEntitlements = await context.UserEntitlements
            .Where(e => e.GardenId == gardenId && e.UserId == userId && !e.IsTrial)
            .ToListAsync(cancellationToken);

        var totalFreeSlots = candidateEntitlements.Count(e => e.ValidToUtc == null || e.ValidToUtc > now);

        var candidateInvitations = await context.GardenInvitations
            .Where(i => i.GardenId == gardenId && i.InvitedByUserId == userId && i.IsFreeSlot && !i.IsRevoked)
            .ToListAsync(cancellationToken);

        var usedFreeSlots = candidateInvitations.Count(i => i.IsAccepted || i.ExpiresUtc > now);

        return new FreeInvitationQuotaDto(totalFreeSlots, usedFreeSlots, Math.Max(0, totalFreeSlots - usedFreeSlots));
    }

    private static bool IsWithinOwnRights(GardenAccessLevel layer, AccessCategory category, GardenMembership own) =>
        (int)layer >= (int)own.Layer && (int)category <= (int)own.Category;

    private static bool IsWithinInvitationCeiling(
        GardenAccessLevel grantedLayer, AccessCategory grantedCategory, GardenInvitation invitation) =>
        (int)grantedLayer >= (int)invitation.MaxAllowedLayer && (int)grantedLayer <= (int)invitation.TargetLayer
        && (int)grantedCategory <= (int)invitation.MaxAllowedCategory && (int)grantedCategory >= (int)invitation.TargetCategory;

    private static DateTimeOffset? ComputeValidTo(BillingCycle billingCycle, DateTimeOffset now) => billingCycle switch
    {
        BillingCycle.Annual => now.AddYears(1),
        BillingCycle.Monthly => now.AddMonths(1),
        BillingCycle.Perpetual => null,
        _ => throw new ArgumentOutOfRangeException(nameof(billingCycle), billingCycle, "Ukendt BillingCycle.")
    };

    private static async Task ApplyAddOnQuantitiesAsync(
        PlannerDbContext context, UserEntitlement entitlement,
        IReadOnlyDictionary<Guid, int> addOnQuantities, CancellationToken cancellationToken)
    {
        if (addOnQuantities.Count == 0)
        {
            return;
        }

        var addOns = await context.SubscriptionAddOns
            .Where(a => addOnQuantities.Keys.Contains(a.Id))
            .ToListAsync(cancellationToken);

        foreach (var (addOnId, quantity) in addOnQuantities)
        {
            var addOn = addOns.SingleOrDefault(a => a.Id == addOnId)
                ?? throw new InvalidOperationException($"Tilkøb med Id {addOnId} findes ikke.");

            switch (addOn.Type)
            {
                case AddOnType.BedforslagNiveau2:
                    entitlement.ExtraBedProposalsCount += quantity;
                    break;
                case AddOnType.PlanlagteBedeNiveau3:
                    entitlement.ExtraPlannedBedsCount += quantity;
                    break;
                case AddOnType.ArtefaktpakkeA:
                    entitlement.ExtraCategoryAArtifactsCount += quantity;
                    break;
                case AddOnType.ArtefaktpakkeB:
                    entitlement.ExtraCategoryBArtifactsCount += quantity;
                    break;
                case AddOnType.BedeINiveau2:
                    // OBS: intet dedikeret kvotefelt på UserEntitlement endnu — kendt gab fra Prompt 1.
                    break;
            }
        }
    }

    private static GardenInvitationDto ToDto(GardenInvitation invitation) => new(
        invitation.Id, invitation.GardenId, invitation.InvitedByUserId, invitation.Email,
        invitation.TargetLayer, invitation.TargetCategory, invitation.IsFreeSlot, invitation.AllowSelfUpgrade,
        invitation.ExpiresUtc, invitation.IsAccepted, invitation.IsRevoked, invitation.CreatedAtUtc,
        invitation.MaxAllowedLayer, invitation.MaxAllowedCategory);

    private static CheckoutDraftDto ToDraftDto(CheckoutDraft draft) => new(
        draft.Id, draft.UserId, draft.GardenName, draft.Description,
        draft.Layer, draft.Category, draft.BillingCycle, draft.AddOnQuantities, draft.ExpiresUtc);
}
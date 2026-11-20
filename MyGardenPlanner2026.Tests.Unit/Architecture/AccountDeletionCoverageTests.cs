namespace MyGardenPlanner2026.Tests.Unit.Architecture;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyGardenPlanner2026.Core.Entities;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Gardens;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data;
using Xunit;

/// <summary>
/// R10.3: sikkerhedsnet for kontosletning. Fejler, når en ny entitet eller bruger-egenskab tilføjes
/// til EF-modellen uden at være klassificeret her. Klassifikationen tvinger udvikleren til at beslutte,
/// hvad der sker med dataene, når en bruger (eller en have) slettes. Se AccountDeletionService.
///
/// Når Layer 2/3-entiteter (bede, planer m.m.) bygges: klassificér dem som GardenOwned og sørg for en
/// cascade-sti til Garden. Testen verificerer stien.
/// </summary>
public sealed class AccountDeletionCoverageTests
{
    private enum EntityKind
    {
        /// <summary>Roden: slettes af AccountDeletionService og oprydnings-sweep'et.</summary>
        GardenRoot,

        /// <summary>Haveejet data: SKAL have en cascade-sti til Garden.</summary>
        GardenOwned,

        /// <summary>Brugerejet data, som AccountDeletionService selv sletter.</summary>
        DeletedWithUser,

        /// <summary>Bevares bevidst ved kontosletning (begrundelse påkrævet).</summary>
        RetainedByDesign,

        /// <summary>Katalog-/konfigurationsdata uden bruger- eller haveejerskab.</summary>
        Global
    }

    private enum UserReferenceKind
    {
        DeletedWithUser,
        RetainedByDesign
    }

    private static readonly Dictionary<Type, (EntityKind Kind, string Reason)> EntityKinds = new()
    {
        [typeof(Garden)] = (EntityKind.GardenRoot,
            "Slettes af AccountDeletionService (eneste medlem/slet for alle) og af GardenRestoreExpirySweepService."),

        [typeof(GardenMembership)] = (EntityKind.GardenOwned,
            "Cascade fra Garden; brugerens egne rækker slettes af AccountDeletionService."),
        [typeof(UserEntitlement)] = (EntityKind.GardenOwned,
            "Cascade fra Garden; brugerens egne rækker slettes af AccountDeletionService."),
        [typeof(GardenInvitation)] = (EntityKind.GardenOwned,
            "Cascade fra Garden; brugerens invitationer slettes/ugyldiggøres af AccountDeletionService."),
        [typeof(PendingGardenRestore)] = (EntityKind.GardenOwned,
            "Cascade fra Garden; ryddes ved gendannelse og af GardenRestoreExpirySweepService."),

        [typeof(CheckoutDraft)] = (EntityKind.DeletedWithUser, "Slettes af AccountDeletionService (UserId)."),
        [typeof(AuditLogViewerPreference)] = (EntityKind.DeletedWithUser,
            "Slettes af AccountDeletionService (R10.3): LastFilterJson kan indeholde e-mails."),
        [typeof(ReAuthFailureAttempt)] = (EntityKind.DeletedWithUser,
            "Ryddes via IReAuthFailureTracker.ClearFailuresAsync (admin-schema)."),

        [typeof(AuditLog)] = (EntityKind.RetainedByDesign, "Append-only revisionslog, bevares 5 år (R2 D4)."),
        [typeof(RoleElevationRequest)] = (EntityKind.RetainedByDesign, "R2 D7: beholdes ved kontosletning."),
        [typeof(AuditLogExportJob)] = (EntityKind.RetainedByDesign,
            "Transient jobdata i admin-schema (kun IAdminDbContextFactory); rækker og filer ryddes fysisk efter ExpiresAtUtc."),

        [typeof(SubscriptionTier)] = (EntityKind.Global, "Abonnementskatalog."),
        [typeof(GardenVolumeDiscountTier)] = (EntityKind.Global, "Abonnementskatalog."),
        [typeof(SubscriptionAddOn)] = (EntityKind.Global, "Abonnementskatalog."),
        [typeof(DummyEntityPlant)] = (EntityKind.Global,
            "Midlertidig stamdata uden bruger- eller haveref. Omklassificér, når Layer 1-stamdata bliver brugerejet."),

        [typeof(JitElevationPolicySettings)] = (EntityKind.Global, "Singleton-sikkerhedspolicy."),
        [typeof(ReAuthenticationPolicySettings)] = (EntityKind.Global, "Singleton-sikkerhedspolicy."),
        [typeof(ReAuthFailureTrackerSettings)] = (EntityKind.Global, "Singleton-sikkerhedspolicy."),
        [typeof(AdminApiRateLimitSettings)] = (EntityKind.Global, "Singleton-sikkerhedspolicy."),
        [typeof(LoginRateLimitSettings)] = (EntityKind.Global, "Singleton-sikkerhedspolicy."),
        [typeof(AuditLogExportJobPolicySettings)] = (EntityKind.Global, "Singleton-sikkerhedspolicy.")
    };

    /// <summary>
    /// Alle egenskaber, der refererer til en bruger (nøgle "Entitet.Egenskab"). "DeletedBy" er undtaget
    /// og bevares altid (soft-delete-aktør, se SoftDeleteInterceptor).
    /// </summary>
    private static readonly Dictionary<string, (UserReferenceKind Kind, string Reason)> UserReferences = new()
    {
        ["GardenMembership.UserId"] = (UserReferenceKind.DeletedWithUser, "AccountDeletionService: slettet fysisk (eller soft-slettet til gendannelse)."),
        ["UserEntitlement.UserId"] = (UserReferenceKind.DeletedWithUser, "AccountDeletionService: slettet fysisk (eller soft-slettet til gendannelse)."),
        ["CheckoutDraft.UserId"] = (UserReferenceKind.DeletedWithUser, "AccountDeletionService."),
        ["GardenInvitation.InvitedByUserId"] = (UserReferenceKind.DeletedWithUser, "AccountDeletionService: accepterede/tilbagekaldte slettes, øvrige ugyldiggøres."),
        ["GardenInvitation.Email"] = (UserReferenceKind.DeletedWithUser, "AccountDeletionService: invitationer sendt TIL brugerens e-mail slettes (D14)."),
        ["AuditLogViewerPreference.UserId"] = (UserReferenceKind.DeletedWithUser, "AccountDeletionService (R10.3)."),
        ["ReAuthFailureAttempt.UserId"] = (UserReferenceKind.DeletedWithUser, "IReAuthFailureTracker.ClearFailuresAsync."),
        ["ReAuthFailureAttempt.IpAddress"] = (UserReferenceKind.DeletedWithUser, "Samme række som ReAuthFailureAttempt.UserId."),

        ["PendingGardenRestore.FormerUserId"] = (UserReferenceKind.RetainedByDesign, "Pseudonymt bruger-id nødvendigt til gendannelse; slettes ved gendannelse og af oprydnings-sweep (R10.1b)."),
        ["PendingGardenRestore.OwnerEmailHash"] = (UserReferenceKind.RetainedByDesign, "Kun SHA-256-hash af e-mailen; slettes sammen med rækken."),
        ["AuditLog.UserId"] = (UserReferenceKind.RetainedByDesign, "Append-only revisionslog, bevares 5 år (R2 D4)."),
        ["AuditLog.UserEmail"] = (UserReferenceKind.RetainedByDesign, "Append-only revisionslog, bevares 5 år (R2 D4)."),
        ["AuditLog.IpAddress"] = (UserReferenceKind.RetainedByDesign, "Append-only revisionslog, bevares 5 år (R2 D4)."),
        ["RoleElevationRequest.RequesterUserId"] = (UserReferenceKind.RetainedByDesign, "R2 D7: RoleElevationRequests beholdes."),
        ["RoleElevationRequest.ApproverUserId"] = (UserReferenceKind.RetainedByDesign, "R2 D7: RoleElevationRequests beholdes."),
        ["AuditLogExportJob.RequestedByUserId"] = (UserReferenceKind.RetainedByDesign, "Transient jobdata; ryddes fysisk efter ExpiresAtUtc.")
    };

    private static IModel Model()
    {
        var options = new DbContextOptionsBuilder<PlannerDbContext>().UseSqlite("DataSource=:memory:").Options;
        using var context = new PlannerDbContext(options);
        return context.Model;
    }

    private static List<IEntityType> ApplicationEntities() =>
        [.. Model().GetEntityTypes().Where(t => !IsIdentity(t))];

    /// <summary>Identity-tabellerne slettes af UserManager.DeleteAsync (FK-cascade fra AspNetUsers).</summary>
    private static bool IsIdentity(IEntityType entityType) =>
        typeof(IdentityUser).IsAssignableFrom(entityType.ClrType)
        || entityType.ClrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity", StringComparison.Ordinal) == true;

    private static bool IsUserReference(string propertyName) =>
        propertyName.EndsWith("UserId", StringComparison.Ordinal)
        || propertyName is "UserEmail" or "Email" or "IpAddress" or "OwnerEmailHash";

    private static List<string> DiscoverUserReferences() =>
        [.. ApplicationEntities()
            .SelectMany(t => t.GetProperties()
                .Where(p => IsUserReference(p.Name))
                .Select(p => $"{t.ClrType.Name}.{p.Name}"))
            .Order(StringComparer.Ordinal)];

    private static bool CascadesToGarden(IEntityType start)
    {
        var visited = new HashSet<IEntityType>();
        var queue = new Queue<IEntityType>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            foreach (var foreignKey in current.GetForeignKeys().Where(f => f.DeleteBehavior == DeleteBehavior.Cascade))
            {
                if (foreignKey.PrincipalEntityType.ClrType == typeof(Garden))
                {
                    return true;
                }

                queue.Enqueue(foreignKey.PrincipalEntityType);
            }
        }

        return false;
    }

    // ---------- Entiteter ----------

    [Fact]
    public void EveryEntityInTheModel_IsClassified()
    {
        var unclassified = ApplicationEntities()
            .Select(t => t.ClrType)
            .Where(t => !EntityKinds.ContainsKey(t))
            .Select(t => t.Name)
            .Order(StringComparer.Ordinal)
            .ToList();

        unclassified.Should().BeEmpty(
            "nye entiteter skal klassificeres i AccountDeletionCoverageTests.EntityKinds (hvad sker der ved kontosletning?). "
            + "Haveejet data skal klassificeres som GardenOwned og have en cascade-sti til Garden. Mangler: {0}",
            string.Join(", ", unclassified));
    }

    [Fact]
    public void EntityClassification_HasNoStaleEntries()
    {
        var inModel = ApplicationEntities().Select(t => t.ClrType).ToHashSet();

        var stale = EntityKinds.Keys.Where(t => !inModel.Contains(t)).Select(t => t.Name).ToList();

        stale.Should().BeEmpty("entiteter, der ikke længere findes i modellen, skal fjernes fra EntityKinds. Forældet: {0}", string.Join(", ", stale));
    }

    [Fact]
    public void GardenOwnedEntities_CascadeFromGarden()
    {
        var withoutCascade = ApplicationEntities()
            .Where(t => EntityKinds.TryGetValue(t.ClrType, out var info) && info.Kind == EntityKind.GardenOwned)
            .Where(t => !CascadesToGarden(t))
            .Select(t => t.ClrType.Name)
            .ToList();

        withoutCascade.Should().BeEmpty(
            "haveejet data skal slettes med haven (FK med DeleteBehavior.Cascade til Garden, evt. via en anden haveejet entitet). Mangler: {0}",
            string.Join(", ", withoutCascade));
    }

    [Fact]
    public void EntitiesWithGardenId_AreClassifiedAsGardenOwned()
    {
        var misclassified = ApplicationEntities()
            .Where(t => t.ClrType != typeof(Garden) && t.FindProperty("GardenId") is not null)
            .Where(t => !EntityKinds.TryGetValue(t.ClrType, out var info) || info.Kind != EntityKind.GardenOwned)
            .Select(t => t.ClrType.Name)
            .ToList();

        misclassified.Should().BeEmpty(
            "entiteter med GardenId skal være GardenOwned, så de slettes med haven. Forkert klassificeret: {0}",
            string.Join(", ", misclassified));
    }

    [Fact]
    public void EveryClassificationHasAReason()
    {
        EntityKinds.Where(e => string.IsNullOrWhiteSpace(e.Value.Reason)).Select(e => e.Key.Name)
            .Should().BeEmpty();
        UserReferences.Where(e => string.IsNullOrWhiteSpace(e.Value.Reason)).Select(e => e.Key)
            .Should().BeEmpty();
    }

    // ---------- Bruger-referencer ----------

    [Fact]
    public void EveryUserReferenceInTheModel_IsClassified()
    {
        var unclassified = DiscoverUserReferences().Where(r => !UserReferences.ContainsKey(r)).ToList();

        unclassified.Should().BeEmpty(
            "nye bruger-egenskaber skal klassificeres i AccountDeletionCoverageTests.UserReferences: slettes de af "
            + "AccountDeletionService (DeletedWithUser), eller bevares de bevidst med begrundelse (RetainedByDesign)? Mangler: {0}",
            string.Join(", ", unclassified));
    }

    [Fact]
    public void UserReferenceClassification_HasNoStaleEntries()
    {
        var discovered = DiscoverUserReferences().ToHashSet();

        var stale = UserReferences.Keys.Where(k => !discovered.Contains(k)).ToList();

        stale.Should().BeEmpty("egenskaber, der ikke længere findes i modellen, skal fjernes fra UserReferences. Forældet: {0}", string.Join(", ", stale));
    }

    [Fact]
    public void EntitiesDeletedWithUser_AreExactlyThoseWithDeletedWithUserReferences()
    {
        // Konsistens: en entitet klassificeret som slettet med brugeren skal have mindst én DeletedWithUser-reference,
        // så klassifikationerne ikke kan modsige hinanden.
        var entitiesWithDeletedReference = UserReferences
            .Where(r => r.Value.Kind == UserReferenceKind.DeletedWithUser)
            .Select(r => r.Key.Split('.')[0])
            .ToHashSet();

        var inconsistent = EntityKinds
            .Where(e => e.Value.Kind == EntityKind.DeletedWithUser)
            .Select(e => e.Key.Name)
            .Where(name => !entitiesWithDeletedReference.Contains(name))
            .ToList();

        inconsistent.Should().BeEmpty(
            "entiteter klassificeret som DeletedWithUser skal have en DeletedWithUser-reference i UserReferences. Mangler: {0}",
            string.Join(", ", inconsistent));
    }
}
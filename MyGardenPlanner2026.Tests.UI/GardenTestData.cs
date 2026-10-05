namespace MyGardenPlanner2026.Tests.UI;

using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Fælles builders til garden-DTO'er i UI-tests. Alle parametre har defaults, så tests kun angiver det, de tester.
/// </summary>
public static class GardenTestData
{
    /// <summary>
    /// Medlemskab. Default: ejer med Have Arkitekt / Administrator for "user-1".
    /// </summary>
    public static GardenMembershipDto Membership(
        Guid? gardenId = null,
        string userId = "user-1",
        bool isOwner = true,
        GardenAccessLevel level = GardenAccessLevel.HaveArkitekt,
        AccessCategory category = AccessCategory.Administrator) =>
        new(Guid.NewGuid(), gardenId ?? Guid.NewGuid(), userId, isOwner, level, category, DateTimeOffset.UtcNow);

    /// <summary>
    /// Invitation. Default: afventende, udløber om 7 dage, Bed Designer / Editor.
    /// Loftet (<paramref name="maxLayer"/>/<paramref name="maxCategory"/>) følger målet, medmindre andet angives.
    /// </summary>
    public static GardenInvitationDto Invitation(
        Guid? id = null,
        Guid? gardenId = null,
        string invitedByUserId = "owner",
        string invitedEmail = "invited@example.com",
        GardenAccessLevel targetLayer = GardenAccessLevel.BedDesigner,
        AccessCategory targetCategory = AccessCategory.Editor,
        bool allowSelfUpgrade = false,
        DateTimeOffset? expiresUtc = null,
        bool isAccepted = false,
        bool isRevoked = false,
        GardenAccessLevel? maxLayer = null,
        AccessCategory? maxCategory = null) =>
        new(id ?? Guid.NewGuid(), gardenId ?? Guid.NewGuid(), invitedByUserId, invitedEmail,
            targetLayer, targetCategory, false, allowSelfUpgrade,
            expiresUtc ?? DateTimeOffset.UtcNow.AddDays(7), isAccepted, isRevoked, DateTimeOffset.UtcNow,
            maxLayer ?? targetLayer, maxCategory ?? targetCategory);

    /// <summary>Have-resumé (ikke arkiveret).</summary>
    public static GardenSummaryDto Summary(Guid gardenId, string name = "Testhave") =>
        new(gardenId, name, false);
}
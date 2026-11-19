namespace MyGardenPlanner2026.Components.Domain.Subscriptions;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Entities.Common;
using System.Globalization;

/// <summary>
/// Valg mellem "opret ny have" og gendannelse af en gemt have (R10.1e). Haver, hvis tidligere
/// ejerniveau ikke dækkes af det valgte køb (GardenRestoreCoverage), er deaktiveret med en
/// opgraderingsbesked. Forælderen styrer valget via @bind-SelectedPendingRestoreId.
/// </summary>
public partial class RestoreGardenChoice
{
    [Parameter, EditorRequired] public IReadOnlyList<RestorableGardenDto> Gardens { get; set; } = default!;

    /// <summary>Det valgte købs niveau, som skal dække havens tidligere niveau.</summary>
    [Parameter, EditorRequired] public GardenAccessLevel SelectedLayer { get; set; }

    [Parameter, EditorRequired] public AccessCategory SelectedCategory { get; set; }

    /// <summary>Null = opret ny have.</summary>
    [Parameter] public Guid? SelectedPendingRestoreId { get; set; }

    [Parameter] public EventCallback<Guid?> SelectedPendingRestoreIdChanged { get; set; }

    /// <summary>Rejses ved klik på "Ændr abonnement" (tilbage til valg af abonnement).</summary>
    [Parameter] public EventCallback OnChangeSubscription { get; set; }

    private bool IsCovered(RestorableGardenDto garden) =>
        GardenRestoreCoverage.Covers(SelectedLayer, SelectedCategory, garden.Layer, garden.Category);

    private Task SelectAsync(Guid? pendingRestoreId) =>
        SelectedPendingRestoreIdChanged.InvokeAsync(pendingRestoreId);

    private static string ExpiryLabel(RestorableGardenDto garden) =>
        garden.ExpiresUtc.UtcDateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) + " (UTC)";

    private static string RequiredLabel(RestorableGardenDto garden) =>
        $"{garden.Layer.ToDisplayName()} / {garden.Category.ToDisplayName()}";
}
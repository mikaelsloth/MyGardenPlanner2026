namespace MyGardenPlanner2026.Services;

using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Scoped Blazor state-container der fastholder brugerens valg gennem onboarding-guiden
/// (valgt Layer/Category, havenavn, tilkøb, betalingsfrekvens og evt. aktivt invitations-
/// token), så tilstanden overlever navigation mellem onboarding-trinnene inden for samme
/// circuit. Registreres Scoped via AddOnboardingServices().
/// </summary>
public sealed class OnboardingStateContainer
{
    public GardenAccessLevel? SelectedLayer { get; set; }
    public AccessCategory? SelectedCategory { get; set; }
    public BillingCycle? SelectedBillingCycle { get; set; }

    public string? GardenName { get; set; }
    public string? GardenDescription { get; set; }

    /// <summary>Valgte tilkøbsmoduler og deres antal, nøglet på SubscriptionAddOn.Id.</summary>
    public Dictionary<Guid, int> SelectedAddOnQuantities { get; } = [];

    /// <summary>Det rå (ikke-hashede) invitations-token, hvis onboarding sker via en invitation.</summary>
    public string? ActiveInvitationToken { get; set; }

    public bool HasActiveInvitation => !string.IsNullOrWhiteSpace(ActiveInvitationToken);

    /// <summary>Nulstiller al gemt onboarding-state, fx efter gennemført provisionering.</summary>
    public void Reset()
    {
        SelectedLayer = null;
        SelectedCategory = null;
        SelectedBillingCycle = null;
        GardenName = null;
        GardenDescription = null;
        SelectedAddOnQuantities.Clear();
        ActiveInvitationToken = null;
    }
}
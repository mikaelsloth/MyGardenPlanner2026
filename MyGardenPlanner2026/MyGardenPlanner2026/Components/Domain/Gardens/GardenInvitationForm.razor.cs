namespace MyGardenPlanner2026.Components.Domain.Gardens;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Formular til oprettelse af en ny haveinvitation. TargetLayer/TargetCategory-valg
/// begrænses altid til afsenderens (RequesterMembership) egne rettigheder — det samme
/// håndhæves serverside i OnboardingService som en sikkerhedsforanstaltning mod
/// manipuleret klientdata.
/// </summary>
public partial class GardenInvitationForm
{
    [Inject] private IOnboardingService OnboardingService { get; set; } = default!;

    [Parameter, EditorRequired] public Guid GardenId { get; set; }
    [Parameter, EditorRequired] public GardenMembershipDto RequesterMembership { get; set; } = default!;
    [Parameter, EditorRequired] public FreeInvitationQuotaDto FreeQuota { get; set; } = default!;
    [Parameter] public EventCallback<(CreateInvitationResultDto Result, string Email)> OnInvitationCreated { get; set; }

    private string email = string.Empty;
    private GardenAccessLevel selectedLayer;
    private AccessCategory selectedCategory;
    private bool allowSelfUpgrade;
    private bool useFreeSlot;
    private bool isSubmitting;
    private string? errorMessage;

    protected override void OnParametersSet()
    {
        selectedLayer = RequesterMembership.Layer;
        selectedCategory = RequesterMembership.Category;
    }

    private IEnumerable<GardenAccessLevel> AllowedLayers =>
        Enum.GetValues<GardenAccessLevel>().Where(l => (int)l >= (int)RequesterMembership.Layer);

    private IEnumerable<AccessCategory> AllowedCategories =>
        Enum.GetValues<AccessCategory>().Where(c => (int)c <= (int)RequesterMembership.Category);

    private bool CanUseFreeSlot =>
        FreeQuota.RemainingFreeSlots > 0
        && selectedLayer == RequesterMembership.Layer
        && selectedCategory == RequesterMembership.Category;

    private async Task SubmitAsync()
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(email))
        {
            errorMessage = "Angiv venligst modtagerens e-mail.";
            return;
        }

        isSubmitting = true;
        try
        {
            var maxLayer = allowSelfUpgrade ? RequesterMembership.Layer : selectedLayer;
            var maxCategory = allowSelfUpgrade ? RequesterMembership.Category : selectedCategory;

            var request = new CreateInvitationRequestDto(
                GardenId, RequesterMembership.UserId, email,
                selectedLayer, selectedCategory, maxLayer, maxCategory,
                AllowSelfUpgrade: allowSelfUpgrade,
                UseFreeSlot: useFreeSlot && CanUseFreeSlot,
                ValidFor: TimeSpan.FromDays(7));

            var result = await OnboardingService.CreateInvitationAsync(request);

            var invitedEmail = email;
            email = string.Empty;
            allowSelfUpgrade = false;
            useFreeSlot = false;

            await OnInvitationCreated.InvokeAsync((result, invitedEmail));
        }
        catch (InvalidOperationException ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
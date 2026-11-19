namespace MyGardenPlanner2026.Components.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using System.Globalization;

public partial class OnboardingWelcomePage
{
    [Inject] private IOnboardingService OnboardingService { get; set; } = default!;
    [Inject] private IGardenRestoreService RestoreService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    private bool isCreatingSandbox;
    private string? errorMessage;
    private IReadOnlyList<RestorableGardenDto> restorableGardens = [];

    private string RestorableNames => string.Join(", ", restorableGardens.Select(g => g.Name));

    private string NearestExpiryLabel =>
        restorableGardens.Min(g => g.ExpiresUtc).UtcDateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) + " (UTC)";

    protected override async Task OnInitializedAsync()
    {
        // R10.1e: gemte haver kan gendannes via et betalt køb (ikke via prøveperioden).
        var email = await CurrentUserEmailResolver.ResolveAsync(AuthenticationStateTask);
        if (!string.IsNullOrWhiteSpace(email))
        {
            restorableGardens = await RestoreService.GetRestorableGardensAsync(email);
        }
    }

    private async Task CreateSandboxAsync()
    {
        errorMessage = null;

        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);
        if (userId is null)
        {
            errorMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";
            return;
        }

        isCreatingSandbox = true;
        try
        {
            await OnboardingService.CreateSandboxGardenAsync(userId);
            NavigationManager.NavigateTo("/demo-dashboard");
        }
        finally
        {
            isCreatingSandbox = false;
        }
    }

    private void GoToCheckout() => NavigationManager.NavigateTo("/onboarding/checkout");
}
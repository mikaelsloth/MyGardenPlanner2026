namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Components.Account.Shared;
using MyGardenPlanner2026.Core.Contracts.Onboarding;

/// <summary>
/// Gateway-tjek for AppShell-sider: sender brugere uden nogen aktiv haveadgang (jf.
/// IGardenAccessQueryService.HasAnyGardenAccessAsync) videre til onboarding-welcome-siden
/// i stedet for at vise ChildContent. Kører på hver navigation inden for AppShell (ingen
/// caching) — enkelt og testbart; kan optimeres senere hvis DB-belastning bliver et problem.
/// Uautentificerede brugere rammer i praksis aldrig dette — sider under AppShell har deres
/// eget [Authorize], som Router's AuthorizeRouteView håndterer FØR layoutet renderes — men
/// tjekket er defensivt for robusthedens skyld.
/// </summary>
public partial class OnboardingGate
{
    [Inject] private IGardenAccessQueryService QueryService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool isChecking = true;
    private bool hasAccess;

    protected override async Task OnParametersSetAsync()
    {
        var userId = await CurrentUserIdResolver.ResolveAsync(AuthenticationStateTask);

        if (userId is null)
        {
            isChecking = false;
            hasAccess = false;
            return;
        }

        hasAccess = await QueryService.HasAnyGardenAccessAsync(userId);
        isChecking = false;

        if (!hasAccess)
        {
            NavigationManager.NavigateTo("/onboarding/welcome");
        }
    }
}
namespace MyGardenPlanner2026.Components.Account.Shared;

public partial class ManageNavMenu
{
    private bool hasExternalLogins;

    protected override async Task OnInitializedAsync()
    {
        hasExternalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).Any();
    }
}

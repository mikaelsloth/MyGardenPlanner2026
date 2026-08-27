namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities;

public partial class ExternalLoginPicker
{
    private AuthenticationScheme[] externalLogins = [];

    [Inject]
    private SignInManager<ApplicationUser> SignInManager { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        externalLogins = (await SignInManager.GetExternalAuthenticationSchemesAsync()).ToArray();
    }
}
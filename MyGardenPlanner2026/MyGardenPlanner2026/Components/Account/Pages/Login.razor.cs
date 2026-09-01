namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Components.Account;
using System.ComponentModel.DataAnnotations;

public partial class Login
{
    private string? errorMessage;
    private EditContext editContext = default!;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        editContext = new EditContext(Input);

        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    public async Task LoginUser()
    {
        if (!string.IsNullOrEmpty(Input.Passkey?.Error))
        {
            errorMessage = $"Error: {Input.Passkey.Error}";
            return;
        }

        SignInResult result;
        if (!string.IsNullOrEmpty(Input.Passkey?.CredentialJson))
        {
            result = await SignInManager.PasskeySignInAsync(Input.Passkey.CredentialJson);
        }
        else
        {
            if (!editContext.Validate())
            {
                return;
            }

            result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        }

        if (result.Succeeded)
        {
            ReAuthenticationService.MarkReAuthenticated();
            await TrackReAuthOutcomeAsync(succeeded: true);
            Logger.LogInformation("User logged in.");
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else if (result.RequiresTwoFactor)
        {
            RedirectManager.RedirectTo(
                "Account/LoginWith2fa",
                new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
        }
        else if (result.IsLockedOut)
        {
            Logger.LogWarning("User account locked out.");
            RedirectManager.RedirectTo("Account/Lockout");
        }
        else
        {
            await TrackReAuthOutcomeAsync(succeeded: false);
            errorMessage = "Error: Invalid login attempt.";
        }
    }

    /// <summary>
    /// Registrerer/rydder fejlede login-forsøg i IReAuthFailureTracker (§4.2). Springes
    /// over hvis Input.Email er tom (passkey-only login) eller brugeren ikke findes —
    /// afslører aldrig brugerens eksistens til klienten.
    /// </summary>
    private async Task TrackReAuthOutcomeAsync(bool succeeded)
    {
        if (string.IsNullOrWhiteSpace(Input.Email))
        {
            return;
        }

        var user = await UserManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            return;
        }

        if (succeeded)
        {
            await ReAuthFailureTracker.ClearFailuresAsync(user.Id);
        }
        else
        {
            var ip = CurrentUserAccessor.GetCurrent().IpAddress;
            await ReAuthFailureTracker.RecordFailureAsync(user.Id, ip);
        }
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public PasskeyInputModel? Passkey { get; set; }
    }
}
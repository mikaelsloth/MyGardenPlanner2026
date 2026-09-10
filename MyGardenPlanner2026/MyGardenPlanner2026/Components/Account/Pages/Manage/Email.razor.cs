namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MyGardenPlanner2026.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

public partial class Email
{
    private string? message;

    private ApplicationUser? user;

    private string? email;

    private bool isEmailConfirmed;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm(FormName = "change-email")]
    private InputModel Input { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        email = await UserManager.GetEmailAsync(user);
        isEmailConfirmed = await UserManager.IsEmailConfirmedAsync(user);

        Input.NewEmail ??= email;
    }

    private async Task OnValidSubmitAsync()
    {
        if (Input.NewEmail is null || Input.NewEmail == email)
        {
            message = "Your email is unchanged.";
            return;
        }

        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["email"] = Input.NewEmail, ["code"] = code });

        await EmailSender.SendConfirmationLinkAsync(user, Input.NewEmail, HtmlEncoder.Default.Encode(callbackUrl));

        message = "Confirmation link to change email sent. Please check your email.";
    }

    private async Task OnSendEmailVerificationAsync()
    {
        if (email is null)
        {
            return;
        }

        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });

        await EmailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));

        message = "Verification email sent. Please check your email.";
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string? NewEmail { get; set; }
    }
}

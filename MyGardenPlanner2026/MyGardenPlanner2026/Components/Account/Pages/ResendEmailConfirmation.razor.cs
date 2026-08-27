namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

public partial class ResendEmailConfirmation
{
    private string? message;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    protected override void OnInitialized()
    {
        Input ??= new();
    }

    private async Task OnValidSubmitAsync()
    {
        var user = await UserManager.FindByEmailAsync(Input.Email!);
        if (user is null)
        {
            message = "Bekræftelses-e-mail sendt. Tjek venligst din e-mail.";
            return;
        }

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
        await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

        message = "Bekræftelses-e-mail sendt. Tjek venligst din e-mail.";
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}

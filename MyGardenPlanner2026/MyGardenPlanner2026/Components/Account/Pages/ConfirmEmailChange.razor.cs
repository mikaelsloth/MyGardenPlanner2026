namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public partial class ConfirmEmailChange
{
    private string? message;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? UserId { get; set; }

    [SupplyParameterFromQuery]
    private string? Email { get; set; }

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (UserId is null || Email is null || Code is null)
        {
            RedirectManager.RedirectToWithStatus(
                "Account/Login", "Error: Ugyldigt link til bekræftelse af e-mailskift.", HttpContext);
            return;
        }

        var user = await UserManager.FindByIdAsync(UserId);
        if (user is null)
        {
            message = $"Error: Kunne ikke finde bruger med ID '{UserId}'.";
            return;
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        var result = await UserManager.ChangeEmailAsync(user, Email, code);
        if (!result.Succeeded)
        {
            message = "Error: Der opstod en fejl ved skift af e-mail.";
            return;
        }

        var setUserNameResult = await UserManager.SetUserNameAsync(user, Email);
        if (!setUserNameResult.Succeeded)
        {
            message = "Error: Der opstod en fejl ved opdatering af brugernavn.";
            return;
        }

        await SignInManager.RefreshSignInAsync(user);
        message = "Tak, fordi du bekræftede skiftet af din e-mail.";
    }
}

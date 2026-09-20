namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

public partial class ConfirmEmail
{
    private string? statusMessage;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? UserId { get; set; }

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    private bool confirmationSucceeded;

    private string LoginUrl => string.IsNullOrWhiteSpace(ReturnUrl)
        ? "Account/Login"
        : $"Account/Login?ReturnUrl={Uri.EscapeDataString(ReturnUrl)}";

    protected override async Task OnInitializedAsync()
    {
        if (UserId is null || Code is null)
        {
            RedirectManager.RedirectTo("");
            return;
        }

        var user = await UserManager.FindByIdAsync(UserId);
        if (user is null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            statusMessage = $"Error: Kunne ikke finde bruger med ID {UserId}.";
        }
        else
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
            var result = await UserManager.ConfirmEmailAsync(user, code);
            statusMessage = result.Succeeded
                ? "Tak, fordi du bekræftede din e-mail."
                : "Error: Der opstod en fejl ved bekræftelse af din e-mail.";
            confirmationSucceeded = result.Succeeded;
        }
    }
}

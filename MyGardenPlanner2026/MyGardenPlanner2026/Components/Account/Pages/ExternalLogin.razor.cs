namespace MyGardenPlanner2026.Components.Account.Pages;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MyGardenPlanner2026.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

public partial class ExternalLogin
{
    public const string LoginCallbackAction = "LoginCallback";

    private string? message;

    private ExternalLoginInfo? externalLoginInfo;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? RemoteError { get; set; }

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery]
    private string? Action { get; set; }

    private string? ProviderDisplayName => externalLoginInfo?.ProviderDisplayName;

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        if (RemoteError is not null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", $"Error: Fejl fra ekstern udbyder: {RemoteError}", HttpContext);
            return;
        }

        var info = await SignInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error: Kunne ikke indlæse oplysninger om ekstern login.", HttpContext);
            return;
        }

        externalLoginInfo = info;

        if (HttpMethods.IsGet(HttpContext.Request.Method))
        {
            if (Action == LoginCallbackAction)
            {
                await OnLoginCallbackAsync();
                return;
            }

            RedirectManager.RedirectTo("Account/Login");
        }
    }

    private async Task OnLoginCallbackAsync()
    {
        if (externalLoginInfo is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error: Kunne ikke indlæse oplysninger om ekstern login.", HttpContext);
            return;
        }

        var result = await SignInManager.ExternalLoginSignInAsync(
            externalLoginInfo.LoginProvider,
            externalLoginInfo.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (result.Succeeded)
        {
            Logger.LogInformation(
                "{Name} logged in with {LoginProvider} provider.",
                externalLoginInfo.Principal.Identity?.Name,
                externalLoginInfo.LoginProvider);
            RedirectManager.RedirectTo(ReturnUrl);
            return;
        }
        else if (result.IsLockedOut)
        {
            RedirectManager.RedirectTo("Account/Lockout");
            return;
        }

        if (externalLoginInfo.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            Input.Email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
        }
    }

    private async Task OnValidSubmitAsync()
    {
        if (externalLoginInfo is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Login", "Error: Kunne ikke indlæse oplysninger om ekstern login under bekræftelse.", HttpContext);
            return;
        }

        var emailStore = GetEmailStore();
        var user = CreateUser();

        await UserStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await UserManager.CreateAsync(user);
        if (result.Succeeded)
        {
            result = await UserManager.AddLoginAsync(user, externalLoginInfo);
            if (result.Succeeded)
            {
                Logger.LogInformation("User created an account using {Name} provider.", externalLoginInfo.LoginProvider);

                var userId = await UserManager.GetUserIdAsync(user);
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = NavigationManager.GetUriWithQueryParameters(
                    NavigationManager.ToAbsoluteUri("Account/ConfirmEmail").AbsoluteUri,
                    new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code });
                await EmailSender.SendConfirmationLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));

                if (UserManager.Options.SignIn.RequireConfirmedAccount)
                {
                    RedirectManager.RedirectTo("Account/RegisterConfirmation", new() { ["email"] = Input.Email });
                }
                else
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, externalLoginInfo.LoginProvider);
                    RedirectManager.RedirectTo(ReturnUrl);
                }
            }
        }
        else
        {
            message = $"Error: {string.Join(",", result.Errors.Select(error => error.Description))}";
        }
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        return !UserManager.SupportsUserEmail
            ? throw new NotSupportedException("The default UI requires a user store with email support.")
            : (IUserEmailStore<ApplicationUser>)UserStore;
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}

namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities;
using System;
using System.Buffers.Text;

public partial class Passkeys
{
    private const int MaxPasskeyCount = 100;

    private ApplicationUser? user;

    private IList<UserPasskeyInfo>? currentPasskeys;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private string? Action { get; set; }

    [SupplyParameterFromForm]
    private string? CredentialId { get; set; }

    [SupplyParameterFromForm(FormName = "add-passkey")]
    private PasskeyInputModel Input { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        currentPasskeys = await UserManager.GetPasskeysAsync(user);
    }

    private async Task AddPasskeyAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        if (!string.IsNullOrEmpty(Input.Error))
        {
            RedirectManager.RedirectToCurrentPageWithStatus($"Error: {Input.Error}", HttpContext);
            return;
        }

        if (string.IsNullOrEmpty(Input.CredentialJson))
        {
            RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasskeyBrowserDidNotProvide, HttpContext);
            return;
        }

        if (currentPasskeys!.Count >= MaxPasskeyCount)
        {
            RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasskeyLimitReached, HttpContext);
            return;
        }

        var attestationResult = await SignInManager.PerformPasskeyAttestationAsync(Input.CredentialJson);
        if (!attestationResult.Succeeded)
        {
            RedirectManager.RedirectToCurrentPageWithStatus(
                AccountMessages.PasskeyNotAdded(attestationResult.Failure.Message), HttpContext);
            return;
        }

        var addPasskeyResult = await UserManager.AddOrUpdatePasskeyAsync(user, attestationResult.Passkey);
        if (!addPasskeyResult.Succeeded)
        {
            RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasskeyNotAddedToAccount, HttpContext);
            return;
        }

        // Immediately prompt the user to enter a name for the credential
        var credentialIdBase64Url = Base64Url.EncodeToString(attestationResult.Passkey.CredentialId);
        RedirectManager.RedirectTo($"Account/Manage/RenamePasskey/{credentialIdBase64Url}");
    }

    private async Task UpdatePasskeyAsync()
    {
        switch (Action)
        {
            case "rename":
                RedirectManager.RedirectTo($"Account/Manage/RenamePasskey/{CredentialId}");
                break;
            case "delete":
                await DeletePasskeyAsync();
                break;
            default:
                RedirectManager.RedirectToCurrentPageWithStatus($"Error: Unknown action '{Action}'.", HttpContext);
                break;
        }
    }

    private async Task DeletePasskeyAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(CredentialId);
        }
        catch (FormatException)
        {
            RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.InvalidPasskeyId, HttpContext);
            return;
        }

        var result = await UserManager.RemovePasskeyAsync(user, credentialId);
        if (!result.Succeeded)
        {
            RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasskeyDeleteFailed, HttpContext);
            return;
        }

        RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasskeyDeleted, HttpContext);
    }
}

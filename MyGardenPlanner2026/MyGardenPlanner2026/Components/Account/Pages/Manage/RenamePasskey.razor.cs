namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MyGardenPlanner2026.Core.Entities;
using System;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;

public partial class RenamePasskey
{
    private ApplicationUser? user;

    private UserPasskeyInfo? passkey;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = (await UserManager.GetUserAsync(HttpContext.User))!;
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(Id);
        }
        catch (FormatException)
        {
            RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", AccountMessages.InvalidPasskeyId, HttpContext);
            return;
        }

        passkey = await UserManager.GetPasskeyAsync(user, credentialId);
        if (passkey is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", AccountMessages.PasskeyNotFound, HttpContext);
            return;
        }
    }

    private async Task RenameAsync()
    {
        passkey!.Name = Input.Name;
        var result = await UserManager.AddOrUpdatePasskeyAsync(user!, passkey);
        if (!result.Succeeded)
        {
            RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", AccountMessages.PasskeyUpdateFailed, HttpContext);
            return;
        }

        RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", AccountMessages.PasskeyUpdated, HttpContext);
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = AccountMessages.FieldRequired)]
        [StringLength(200, ErrorMessage = AccountMessages.PasskeyNameTooLong)]
        [Display(Name = "Passkey-navn")]
        public string Name { get; set; } = "";
    }
}

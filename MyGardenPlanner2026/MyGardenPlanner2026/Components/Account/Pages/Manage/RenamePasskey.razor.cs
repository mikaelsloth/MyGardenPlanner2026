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
            RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", "Error: The specified passkey ID had an invalid format.", HttpContext);
            return;
        }

        passkey = await UserManager.GetPasskeyAsync(user, credentialId);
        if (passkey is null)
        {
            RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", "Error: The specified passkey could not be found.", HttpContext);
            return;
        }
    }

    private async Task RenameAsync()
    {
        passkey!.Name = Input.Name;
        var result = await UserManager.AddOrUpdatePasskeyAsync(user!, passkey);
        if (!result.Succeeded)
        {
            RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", "Error: The passkey could not be updated.", HttpContext);
            return;
        }

        RedirectManager.RedirectToWithStatus("Account/Manage/Passkeys", "Passkey updated successfully.", HttpContext);
    }

    private sealed class InputModel
    {
        [Required]
        [StringLength(200, ErrorMessage = "Passkey names must be no longer than {1} characters.")]
        public string Name { get; set; } = "";
    }
}

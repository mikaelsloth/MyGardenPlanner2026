namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities;
using System.ComponentModel.DataAnnotations;

public partial class SetPassword
{
    private string? message;

    private ApplicationUser? user;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
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

        var hasPassword = await UserManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            RedirectManager.RedirectTo("Account/Manage/ChangePassword");
        }
    }

    private async Task OnValidSubmitAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        var addPasswordResult = await UserManager.AddPasswordAsync(user, Input.NewPassword!);
        if (!addPasswordResult.Succeeded)
        {
            message = $"Error: {string.Join(",", addPasswordResult.Errors.Select(error => error.Description))}";
            return;
        }

        await SignInManager.RefreshSignInAsync(user);
        RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasswordsWasSet, HttpContext);
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = AccountMessages.FieldRequired)]
        [StringLength(100, ErrorMessage = AccountMessages.PasswordsLength, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Ny adgangskode")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Bekræft ny adgangskode")]
        [Compare(nameof(NewPassword), ErrorMessage = AccountMessages.PasswordsDoNotMatch)]
        public string? ConfirmPassword { get; set; }
    }
}

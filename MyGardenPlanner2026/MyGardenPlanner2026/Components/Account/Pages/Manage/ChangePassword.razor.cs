namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities;
using System.ComponentModel.DataAnnotations;

public partial class ChangePassword
{
    private string? message;

    private ApplicationUser? user;

    private bool hasPassword;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [LoggerMessage(EventId = 1018, Level = LogLevel.Information, Message = "User with ID '{UserId}' changed their password successfully.")]
    static partial void PasswordChanged(ILogger logger, string UserId);

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        hasPassword = await UserManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            RedirectManager.RedirectTo("Account/Manage/SetPassword");
        }
    }

    private async Task OnValidSubmitAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        var changePasswordResult = await UserManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            message = $"Error: {string.Join(",", changePasswordResult.Errors.Select(error => error.Description))}";
            return;
        }

        await SignInManager.RefreshSignInAsync(user);
        var userId = await UserManager.GetUserIdAsync(user);
        PasswordChanged(Logger, userId);

        RedirectManager.RedirectToCurrentPageWithStatus(AccountMessages.PasswordsWasChanged, HttpContext);
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = AccountMessages.FieldRequired)]
        [DataType(DataType.Password)]
        [Display(Name = "Nuværende adgangskode")]
        public string OldPassword { get; set; } = "";

        [Required(ErrorMessage = AccountMessages.FieldRequired)]
        [StringLength(100, ErrorMessage = AccountMessages.PasswordsLength, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Ny adgangskode")]
        public string NewPassword { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Bekræft ny adgangskode")]
        [Compare(nameof(NewPassword), ErrorMessage = AccountMessages.PasswordsDoNotMatch)]
        public string ConfirmPassword { get; set; } = "";
    }
}

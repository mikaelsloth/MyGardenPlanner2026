namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

public partial class DeletePersonalData
{
    private string? message;

    private ApplicationUser? user;

    private bool requirePassword;

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = default!;

    [LoggerMessage(EventId = 1019, Level = LogLevel.Information, Message = "User with ID '{UserId}' deleted themselves.")]
    static partial void UserSelfDeleted(ILogger logger, string UserId);

    protected override async Task OnInitializedAsync()
    {
        Input ??= new();

        user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        requirePassword = await UserManager.HasPasswordAsync(user);
    }

    private async Task OnValidSubmitAsync()
    {
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
            return;
        }

        if (requirePassword && !await UserManager.CheckPasswordAsync(user, Input.Password))
        {
            message = "Error: Incorrect password.";
            return;
        }

        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Unexpected error occurred deleting user.");
        }

        await SignInManager.SignOutAsync();

        var userId = await UserManager.GetUserIdAsync(user);
        UserSelfDeleted(Logger, userId);

        RedirectManager.RedirectToCurrentPage();
    }

    private sealed class InputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}

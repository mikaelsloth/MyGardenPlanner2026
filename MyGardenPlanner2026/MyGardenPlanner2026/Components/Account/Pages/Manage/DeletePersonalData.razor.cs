namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Core.Entities;
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

    [LoggerMessage(EventId = 1083, Level = LogLevel.Warning, Message = "Deleting user with ID '{UserId}' failed: {Errors}.")]
    static partial void UserDeleteFailed(ILogger logger, string UserId, string Errors);

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

        var userId = await UserManager.GetUserIdAsync(user);
        var email = await UserManager.GetEmailAsync(user);

        var dataResult = await AccountDeletionService.DeleteUserDataAsync(userId, email, CancellationToken.None);
        if (dataResult.IsBlocked)
        {
            message = "Error: You are the only owner of gardens shared with other members: "
                + string.Join(", ", dataResult.BlockingGardenNames)
                + ". Ownership must be transferred before your account can be deleted.";
            return;
        }

        var result = await UserManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            UserDeleteFailed(Logger, userId, string.Join(", ", result.Errors.Select(e => e.Code)));
            message = "Error: Your account could not be deleted. Please try again.";
            return;
        }

        await SignInManager.SignOutAsync();

        UserSelfDeleted(Logger, userId);

        // D4-2: efter sletning sendes brugeren til forsiden.
        RedirectManager.RedirectTo("");
    }

    private sealed class InputModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}

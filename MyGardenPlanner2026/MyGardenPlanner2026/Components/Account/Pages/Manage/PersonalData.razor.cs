namespace MyGardenPlanner2026.Components.Account.Pages.Manage;

using Microsoft.AspNetCore.Components;

public partial class PersonalData
{
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var user = await UserManager.GetUserAsync(HttpContext.User);
        if (user is null)
        {
            RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
        }
    }
}

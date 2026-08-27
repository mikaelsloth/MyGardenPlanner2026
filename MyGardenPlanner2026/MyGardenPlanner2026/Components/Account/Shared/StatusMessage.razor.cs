namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Components.Account;

public partial class StatusMessage
{
    private string? messageFromCookie;

    [Parameter]
    public string? Message { get; set; }

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    private string? DisplayMessage => Message ?? messageFromCookie;

    // OBS: "Error"-præfikset i selve beskeden skal forblive på engelsk —
    // StartsWith("Error") styrer danger/success. Se memory-noter.
    private string StatusVariant =>
        DisplayMessage is not null && DisplayMessage.StartsWith("Error") ? "danger" : "success";

    protected override void OnInitialized()
    {
        messageFromCookie = HttpContext.Request.Cookies[IdentityRedirectManager.StatusCookieName];

        if (messageFromCookie is not null)
        {
            HttpContext.Response.Cookies.Delete(IdentityRedirectManager.StatusCookieName);
        }
    }
}
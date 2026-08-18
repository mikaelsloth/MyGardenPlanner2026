namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public partial class NavDrawer
{
    private ElementReference drawerElement;
    private bool wasOpen;

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !wasOpen)
        {
            await drawerElement.FocusAsync();
        }

        wasOpen = IsOpen;
    }

    private async Task HandleClose() => await OnClose.InvokeAsync();

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await HandleClose();
        }
    }
}
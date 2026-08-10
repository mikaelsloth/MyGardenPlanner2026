namespace MyGardenPlanner2026.Components.Layout;

using Microsoft.AspNetCore.Components;

public partial class PublicHeader
{
    [Parameter]
    public bool IsMobileMenuOpen { get; set; }

    [Parameter]
    public EventCallback<bool> IsMobileMenuOpenChanged { get; set; }

    private async Task ToggleMobileMenu()
    {
        IsMobileMenuOpen = !IsMobileMenuOpen;
        await IsMobileMenuOpenChanged.InvokeAsync(IsMobileMenuOpen);
    }
}

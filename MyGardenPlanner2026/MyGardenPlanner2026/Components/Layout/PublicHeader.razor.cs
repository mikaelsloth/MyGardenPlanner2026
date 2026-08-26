namespace MyGardenPlanner2026.Components.Layout;

public partial class PublicHeader
{
    private bool isMobileMenuOpen;

    private void ToggleMobileMenu() => isMobileMenuOpen = !isMobileMenuOpen;

    private void CloseMobileMenu() => isMobileMenuOpen = false;
}
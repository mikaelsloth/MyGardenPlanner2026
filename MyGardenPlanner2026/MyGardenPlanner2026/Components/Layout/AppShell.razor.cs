namespace MyGardenPlanner2026.Components.Layout;

public partial class AppShell
{
    private bool isMobileMenuOpen;

    private void ToggleMobileMenu() => isMobileMenuOpen = !isMobileMenuOpen;

    private void CloseMobileMenu() => isMobileMenuOpen = false;
}
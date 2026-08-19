namespace MyGardenPlanner2026.Components.Pages.Admin;

public partial class AdminSubscriptionManagementPage
{
    private enum AdminTab { BasePrices, VolumeDiscounts, AddOns }

    private AdminTab activeTab = AdminTab.BasePrices;
    private string? statusMessage;

    private void SelectTab(AdminTab tab)
    {
        activeTab = tab;
        statusMessage = null;
    }

    private void SetStatusMessage(string message) => statusMessage = message;
}
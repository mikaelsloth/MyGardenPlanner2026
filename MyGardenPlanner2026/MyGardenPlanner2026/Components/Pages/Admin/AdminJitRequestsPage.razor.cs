namespace MyGardenPlanner2026.Components.Pages.Admin;

public partial class AdminJitRequestsPage
{
    private enum AdminTab { Request, Approve }

    private AdminTab activeTab = AdminTab.Request;
    private string? statusMessage;

    private void SelectTab(AdminTab tab)
    {
        activeTab = tab;
        statusMessage = null;
    }

    private void SetStatusMessage(string message) => statusMessage = message;
}
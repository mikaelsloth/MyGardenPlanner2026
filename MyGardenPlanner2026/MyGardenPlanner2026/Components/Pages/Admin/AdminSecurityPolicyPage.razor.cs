namespace MyGardenPlanner2026.Components.Pages.Admin;

public partial class AdminSecurityPolicyPage
{
    private enum AdminTab { JitElevation, ReAuthentication, ReAuthFailureTracker, AdminApiRateLimit, LoginRateLimit, AuditLogExportJob }
    private AdminTab activeTab = AdminTab.JitElevation;
    private string? statusMessage;

    private void SelectTab(AdminTab tab)
    {
        activeTab = tab;
        statusMessage = null;
    }

    private void SetStatusMessage(string message) => statusMessage = message;
}
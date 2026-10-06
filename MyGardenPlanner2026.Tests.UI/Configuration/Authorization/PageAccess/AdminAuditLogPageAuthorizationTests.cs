namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization.PageAccess;

using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public sealed class AdminAuditLogPageAuthorizationTests
{
    [Fact]
    public void AdminAuditLogPage_RequiresAuditViewerPolicy()
    {
        PageAuthorizationAssert.RequiresPolicy<AdminAuditLogPage>(
            AuthorizationServicesExtensions.RequireAuditViewerPolicy);
    }
}
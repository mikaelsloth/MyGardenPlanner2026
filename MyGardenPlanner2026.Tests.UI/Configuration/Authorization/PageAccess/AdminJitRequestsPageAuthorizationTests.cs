namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization.PageAccess;

using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AdminJitRequestsPageAuthorizationTests
{
    [Fact]
    public void AdminJitRequestsPage_RequiresAnyAdminRolePolicy()
    {
        PageAuthorizationAssert.RequiresPolicy<AdminJitRequestsPage>(
            AuthorizationServicesExtensions.RequireAnyAdminRolePolicy);
    }
}
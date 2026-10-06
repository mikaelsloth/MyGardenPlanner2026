namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization.PageAccess;

using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AdminSubscriptionManagementPageAuthorizationTests
{
    [Fact]
    public void AdminSubscriptionManagementPage_RequiresGlobalAdminPolicy()
    {
        PageAuthorizationAssert.RequiresPolicy<AdminSubscriptionManagementPage>(
            AuthorizationServicesExtensions.RequireGlobalAdminPolicy);
    }
}
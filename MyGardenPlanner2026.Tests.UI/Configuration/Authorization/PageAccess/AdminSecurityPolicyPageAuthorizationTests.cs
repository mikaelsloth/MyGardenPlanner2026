namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization.PageAccess;

using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AdminSecurityPolicyPageAuthorizationTests
{
    [Fact]
    public void AdminSecurityPolicyPage_RequiresPolicyAdminPolicy()
    {
        PageAuthorizationAssert.RequiresPolicy<AdminSecurityPolicyPage>(
            AuthorizationServicesExtensions.RequirePolicyAdminPolicy);
    }
}
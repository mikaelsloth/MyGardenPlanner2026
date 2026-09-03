namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AdminSecurityPolicyPageAuthorizationTests
{
    [Fact]
    public void AdminSecurityPolicyPage_RequiresPolicyAdminPolicy()
    {
        var attribute = typeof(AdminSecurityPolicyPage)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(AuthorizationServicesExtensions.RequirePolicyAdminPolicy);
    }
}
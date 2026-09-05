namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AdminJitRequestsPageAuthorizationTests
{
    [Fact]
    public void AdminJitRequestsPage_RequiresAnyAdminRolePolicy()
    {
        var attribute = typeof(AdminJitRequestsPage)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(AuthorizationServicesExtensions.RequireAnyAdminRolePolicy);
    }
}
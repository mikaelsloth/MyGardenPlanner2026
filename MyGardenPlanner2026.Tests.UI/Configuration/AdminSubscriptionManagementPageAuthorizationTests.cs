namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AdminSubscriptionManagementPageAuthorizationTests
{
    [Fact]
    public void AdminSubscriptionManagementPage_RequiresGlobalAdminPolicy()
    {
        var attribute = typeof(AdminSubscriptionManagementPage)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(AuthorizationServicesExtensions.RequireGlobalAdminPolicy);
    }
}
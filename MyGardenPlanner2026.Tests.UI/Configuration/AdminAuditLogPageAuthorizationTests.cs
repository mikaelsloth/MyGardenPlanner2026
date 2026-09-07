namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Components.Pages.Admin;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public sealed class AdminAuditLogPageAuthorizationTests
{
    [Fact]
    public void AdminAuditLogPage_RequiresAuditViewerPolicy()
    {
        var attribute = typeof(AdminAuditLogPage)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.Policy.Should().Be(AuthorizationServicesExtensions.RequireAuditViewerPolicy);
    }
}
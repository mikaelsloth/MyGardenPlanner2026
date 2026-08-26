namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class IdentityServicesExtensionsTests
{
    [Fact]
    public void AddIdentityServices_RegistersRoleManagerForIdentityRole()
    {
        var services = new ServiceCollection();
        services.AddIdentityServices();

        services.Should().Contain(d => d.ServiceType == typeof(RoleManager<IdentityRole>));
    }
}
namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AuthorizationServicesExtensionsTests
{
    [Fact]
    public async Task AddAuthorizationServices_RegistersRequireGlobalAdminPolicy_RequiringSystemAdminRole()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(AuthorizationServicesExtensions.RequireGlobalAdminPolicy);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<RolesAuthorizationRequirement>()
            .Should().ContainSingle(r => r.AllowedRoles.Contains(AuthorizationServicesExtensions.SystemAdminRole));
    }
}
namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class AuthorizationServicesExtensionsTests
{
    [Fact]
    public async Task AddAuthorizationServices_RegistersRequireGlobalAdminPolicy_WithJitRoleRequirementForSystemAdmin()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(AuthorizationServicesExtensions.RequireGlobalAdminPolicy);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<JitRoleRequirement>()
            .Should().ContainSingle(r => r.RequiredRole == AuthorizationServicesExtensions.SystemAdminRole);
    }

    [Fact]
    public async Task AddAuthorizationServices_RegistersRequireDataAdminPolicy_WithJitRoleRequirementForDataAdmin()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(AuthorizationServicesExtensions.RequireDataAdminPolicy);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<JitRoleRequirement>()
            .Should().ContainSingle(r => r.RequiredRole == AuthorizationServicesExtensions.DataAdminRole);
    }

    [Fact]
    public async Task AddAuthorizationServices_RegistersRequirePolicyAdminPolicy_WithJitRoleRequirementForPolicyAdmin()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(AuthorizationServicesExtensions.RequirePolicyAdminPolicy);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<JitRoleRequirement>()
            .Should().ContainSingle(r => r.RequiredRole == AuthorizationServicesExtensions.PolicyAdminRole);
    }

    [Fact]
    public async Task AddAuthorizationServices_RegistersRequireAuditViewerPolicy_WithJitRoleRequirementForAuditViewer()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(AuthorizationServicesExtensions.RequireAuditViewerPolicy);

        policy.Should().NotBeNull();
        policy!.Requirements.OfType<JitRoleRequirement>()
            .Should().ContainSingle(r => r.RequiredRole == AuthorizationServicesExtensions.AuditViewerRole);
    }

    [Theory]
    [InlineData("RequireGlobalAdmin")]
    [InlineData("RequireDataAdmin")]
    [InlineData("RequirePolicyAdmin")]
    [InlineData("RequireAuditViewer")]
    public async Task AddAuthorizationServices_AdminPolicies_AlsoRequireMfaRequirement(string policyName)
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        await using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await policyProvider.GetPolicyAsync(policyName);

        policy!.Requirements.OfType<MfaRequirement>().Should().ContainSingle();
    }

    [Fact]
    public void AddAuthorizationServices_RegistersMfaAuthorizationHandler()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationServices();

        services.Should().Contain(d =>
            d.ServiceType == typeof(IAuthorizationHandler) && d.ImplementationType == typeof(MfaAuthorizationHandler));
    }
}
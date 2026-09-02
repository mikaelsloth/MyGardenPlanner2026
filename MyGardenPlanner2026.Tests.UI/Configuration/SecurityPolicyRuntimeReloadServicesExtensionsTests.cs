namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class SecurityPolicyRuntimeReloadServicesExtensionsTests
{
    [Fact]
    public void AddSecurityPolicyRuntimeReload_RegistersSecurityPolicyChangeSignal_AsSingleton()
    {
        var services = new ServiceCollection();

        services.AddSecurityPolicyRuntimeReload();

        services.Should().Contain(d =>
            d.ServiceType == typeof(ISecurityPolicyChangeSignal) && d.Lifetime == ServiceLifetime.Singleton);
    }

    [Theory]
    [InlineData(typeof(JitElevationPolicyOptions), typeof(JitElevationPolicyOptionsConfigurator))]
    [InlineData(typeof(ReAuthenticationPolicyOptions), typeof(ReAuthenticationPolicyOptionsConfigurator))]
    [InlineData(typeof(ReAuthFailureTrackerOptions), typeof(ReAuthFailureTrackerOptionsConfigurator))]
    [InlineData(typeof(AdminApiRateLimitOptions), typeof(AdminApiRateLimitOptionsConfigurator))]
    [InlineData(typeof(LoginRateLimitOptions), typeof(LoginRateLimitOptionsConfigurator))]
    public void AddSecurityPolicyRuntimeReload_RegistersDbBackedConfigureOptions_ForEachPolicyType(
        Type optionsType, Type configuratorType)
    {
        var services = new ServiceCollection();

        services.AddSecurityPolicyRuntimeReload();

        var configureOptionsType = typeof(IConfigureOptions<>).MakeGenericType(optionsType);
        services.Should().Contain(d => d.ServiceType == configureOptionsType && d.ImplementationType == configuratorType);
    }

    [Theory]
    [InlineData(typeof(JitElevationPolicyOptions))]
    [InlineData(typeof(ReAuthenticationPolicyOptions))]
    [InlineData(typeof(ReAuthFailureTrackerOptions))]
    [InlineData(typeof(AdminApiRateLimitOptions))]
    [InlineData(typeof(LoginRateLimitOptions))]
    public void AddSecurityPolicyRuntimeReload_RegistersChangeTokenSource_ForEachPolicyType(Type optionsType)
    {
        var services = new ServiceCollection();

        services.AddSecurityPolicyRuntimeReload();

        var tokenSourceType = typeof(IOptionsChangeTokenSource<>).MakeGenericType(optionsType);
        services.Should().Contain(d => d.ServiceType == tokenSourceType);
    }

    [Fact]
    public void AddSecurityPolicyRuntimeReload_RegistersAllFiveAdminServices_AsScoped()
    {
        var services = new ServiceCollection();

        services.AddSecurityPolicyRuntimeReload();

        services.Should().Contain(d => d.ServiceType == typeof(IJitElevationPolicyAdminService) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d => d.ServiceType == typeof(IReAuthenticationPolicyAdminService) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d => d.ServiceType == typeof(IReAuthFailureTrackerPolicyAdminService) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d => d.ServiceType == typeof(IAdminApiRateLimitPolicyAdminService) && d.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(d => d.ServiceType == typeof(ILoginRateLimitPolicyAdminService) && d.Lifetime == ServiceLifetime.Scoped);
    }
}
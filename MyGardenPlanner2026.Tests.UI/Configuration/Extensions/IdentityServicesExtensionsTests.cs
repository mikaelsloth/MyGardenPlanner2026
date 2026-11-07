namespace MyGardenPlanner2026.Tests.UI.Configuration.Extensions;

using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Configuration.Extensions;
using Xunit;

public class IdentityServicesExtensionsTests
{
    private static CookieAuthenticationOptions GetApplicationCookieOptions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddIdentityServices();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
    }

    [Fact]
    public void AddIdentityServices_ApplicationCookie_HasExplicitLoginPath() =>
        GetApplicationCookieOptions().LoginPath.Value.Should().Be("/Account/Login");

    [Fact]
    public void AddIdentityServices_ApplicationCookie_HasExplicitAccessDeniedPath() =>
        GetApplicationCookieOptions().AccessDeniedPath.Value.Should().Be("/Account/AccessDenied");

    [Fact]
    public void AddIdentityServices_ApplicationCookie_UsesReturnUrlParameter() =>
        GetApplicationCookieOptions().ReturnUrlParameter.Should().Be("ReturnUrl");

    [Fact]
    public void AddIdentityServices_RegistersRoleManagerForIdentityRole()
    {
        var services = new ServiceCollection();
        services.AddIdentityServices();

        services.Should().Contain(d => d.ServiceType == typeof(RoleManager<IdentityRole>));
    }
}
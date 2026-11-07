namespace MyGardenPlanner2026.Tests.UI.Components.Routing;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Client;
using Xunit;

public class RedirectToLoginTests : BunitContext
{
    [Fact]
    public void RedirectToLogin_OnInitialized_NavigatesToLoginWithRelativeReturnUrl()
    {
        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo("http://localhost/admin/subscriptions?tab=1");

        Render<RedirectToLogin>();

        navMan.Uri.Should().Be("http://localhost/Account/Login?ReturnUrl=%2Fadmin%2Fsubscriptions%3Ftab%3D1");
    }

    [Fact]
    public void RedirectToLogin_ReturnUrl_IsRelative_NotAbsolute()
    {
        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.NavigateTo("http://localhost/admin/jit-requests");

        Render<RedirectToLogin>();

        navMan.Uri.Should().NotContain("localhost%2F").And.NotContain("http%3A");
    }
}
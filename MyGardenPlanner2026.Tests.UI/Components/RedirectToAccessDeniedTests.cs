namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Client;
using Xunit;

public class RedirectToAccessDeniedTests : BunitContext
{
    [Fact]
    public void RedirectToAccessDenied_OnInitialized_NavigatesToAccessDeniedPage()
    {
        Render<RedirectToAccessDenied>();

        var navMan = Services.GetRequiredService<BunitNavigationManager>();
        navMan.Uri.Should().EndWith("Account/AccessDenied");
    }
}
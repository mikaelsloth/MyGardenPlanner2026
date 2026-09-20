namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Layout;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public sealed class OnboardingGateTests : BunitContext
{
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();

    public OnboardingGateTests() => Services.AddSingleton(queryService);

    private void AuthorizeAs(string userId)
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized(userId);
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    [Fact]
    public void Authenticated_HasAccess_RendersChildContent()
    {
        AuthorizeAs("user-1");
        queryService.HasAnyGardenAccessAsync("user-1", Arg.Any<CancellationToken>()).Returns(true);

        var cut = Render<OnboardingGate>(p => p.AddChildContent("<p class=\"gated-content\">Indhold</p>"));

        cut.Find(".gated-content").Should().NotBeNull();
    }

    [Fact]
    public void Authenticated_NoAccess_DoesNotRenderChildContent_AndNavigatesToOnboardingWelcome()
    {
        AuthorizeAs("user-1");
        queryService.HasAnyGardenAccessAsync("user-1", Arg.Any<CancellationToken>()).Returns(false);

        var cut = Render<OnboardingGate>(p => p.AddChildContent("<p class=\"gated-content\">Indhold</p>"));

        cut.FindAll(".gated-content").Should().BeEmpty();
        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain("/onboarding/welcome");
    }

    [Fact]
    public async Task NotAuthenticated_DoesNotRenderChildContent_AndDoesNotNavigate()
    {
        AddAuthorization().SetNotAuthorized();
        var originalUri = Services.GetRequiredService<NavigationManager>().Uri;

        var cut = Render<OnboardingGate>(p => p.AddChildContent("<p class=\"gated-content\">Indhold</p>"));

        cut.FindAll(".gated-content").Should().BeEmpty();
        Services.GetRequiredService<NavigationManager>().Uri.Should().Be(originalUri);
        await queryService.DidNotReceive().HasAnyGardenAccessAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
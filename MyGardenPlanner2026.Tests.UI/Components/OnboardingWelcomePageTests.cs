namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public sealed class OnboardingWelcomePageTests : BunitContext
{
    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();

    public OnboardingWelcomePageTests() => Services.AddSingleton(onboardingService);

    private void AuthorizeAs(string userId)
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized(userId);
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    [Fact]
    public void RendersBothOptionCards()
    {
        AuthorizeAs("user-1");

        var cut = Render<OnboardingWelcomePage>();

        cut.Markup.Should().Contain("Opret gratis prøveperiode");
        cut.Markup.Should().Contain("Vælg abonnement og opret have");
    }

    [Fact]
    public async Task ClickingCreateSandbox_CallsCreateSandboxGardenAsync_AndNavigatesToDemoDashboard()
    {
        AuthorizeAs("user-1");
        onboardingService.CreateSandboxGardenAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new SandboxGardenResultDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        var cut = Render<OnboardingWelcomePage>();
        await cut.Find(".btn-primary").ClickAsync();

        await onboardingService.Received(1).CreateSandboxGardenAsync("user-1", Arg.Any<CancellationToken>());
        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain("/demo-dashboard");
    }

    [Fact]
    public async Task ClickingChooseSubscription_NavigatesToCheckout_WithoutCreatingSandbox()
    {
        AuthorizeAs("user-1");

        var cut = Render<OnboardingWelcomePage>();
        await cut.Find(".btn-secondary").ClickAsync();

        await onboardingService.DidNotReceive().CreateSandboxGardenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Services.GetRequiredService<NavigationManager>().Uri.Should().Contain("/onboarding/checkout");
    }
}
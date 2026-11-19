namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Onboarding;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages;
using MyGardenPlanner2026.Core.Contracts.Account;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public sealed class OnboardingWelcomePageTests : BunitContext
{
    private const string Email = "bruger@example.dk";

    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    private readonly IGardenRestoreService restoreService = Substitute.For<IGardenRestoreService>();

    public OnboardingWelcomePageTests()
    {
        restoreService.GetRestorableGardensAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<RestorableGardenDto>>([]));

        Services.AddSingleton(onboardingService);
        Services.AddSingleton(restoreService);
    }

    private static RestorableGardenDto Restorable(string name, DateTimeOffset expiresUtc) =>
        new(Guid.NewGuid(), Guid.NewGuid(), name, expiresUtc.AddDays(-30), expiresUtc,
            GardenAccessLevel.BedDesigner, AccessCategory.Editor);

    [Fact]
    public void RendersBothOptionCards()
    {
        this.AuthorizeAs("user-1");

        var cut = Render<OnboardingWelcomePage>();

        cut.Markup.Should().Contain("Opret gratis prøveperiode");
        cut.Markup.Should().Contain("Vælg abonnement og opret have");
    }

    [Fact]
    public async Task ClickingCreateSandbox_CallsCreateSandboxGardenAsync_AndNavigatesToDemoDashboard()
    {
        this.AuthorizeAs("user-1");
        onboardingService.CreateSandboxGardenAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(new SandboxGardenResultDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        var cut = Render<OnboardingWelcomePage>();
        await cut.Find(".btn-primary").ClickAsync();

        await onboardingService.Received(1).CreateSandboxGardenAsync("user-1", Arg.Any<CancellationToken>());
        this.ShouldHaveNavigatedTo("/demo-dashboard");
    }

    [Fact]
    public async Task ClickingChooseSubscription_NavigatesToCheckout_WithoutCreatingSandbox()
    {
        this.AuthorizeAs("user-1");

        var cut = Render<OnboardingWelcomePage>();
        await cut.Find(".btn-secondary").ClickAsync();

        await onboardingService.DidNotReceive().CreateSandboxGardenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        this.ShouldHaveNavigatedTo("/onboarding/checkout");
    }

    [Fact]
    public void NoRestorableGardens_ShowsNoRestoreBanner()
    {
        this.AuthorizeAs("user-1", email: Email);

        var cut = Render<OnboardingWelcomePage>();

        cut.FindAll(".status-banner").Should().BeEmpty();
    }

    [Fact]
    public void RestorableGardens_ShowsBannerWithNamesNearestDeadlineAndCheckoutLink()
    {
        this.AuthorizeAs("user-1", email: Email);
        IReadOnlyList<RestorableGardenDto> gardens =
        [
            Restorable("Have A", new DateTimeOffset(2026, 12, 20, 12, 0, 0, TimeSpan.Zero)),
            Restorable("Have B", new DateTimeOffset(2026, 12, 5, 12, 0, 0, TimeSpan.Zero))
        ];
        restoreService.GetRestorableGardensAsync(Email, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(gardens));

        var cut = Render<OnboardingWelcomePage>();

        var banner = cut.Find(".status-banner");
        banner.TextContent.Should().Contain("Have A, Have B");
        banner.TextContent.Should().Contain("05.12.2026 (UTC)");
        banner.QuerySelector("a")!.GetAttribute("href").Should().Be("/onboarding/checkout");
    }

    [Fact]
    public void NoEmailClaim_DoesNotLookUpRestorableGardens()
    {
        this.AuthorizeAs("user-1");

        Render<OnboardingWelcomePage>();

        _ = restoreService.DidNotReceive().GetRestorableGardensAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
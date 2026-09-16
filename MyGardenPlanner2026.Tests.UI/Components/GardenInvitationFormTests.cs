namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Domain.Gardens;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

public sealed class GardenInvitationFormTests : BunitContext
{
    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();

    public GardenInvitationFormTests() => Services.AddSingleton(onboardingService);

    private static GardenMembershipDto CreateRequester(
        GardenAccessLevel layer = GardenAccessLevel.BedDesigner,
        AccessCategory category = AccessCategory.Editor) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "owner", true, layer, category, DateTimeOffset.UtcNow);

    [Fact]
    public void LayerDropdown_OnlyShowsOptionsAtOrBelowRequesterLevel()
    {
        var requester = CreateRequester(layer: GardenAccessLevel.BedDesigner);
        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, Guid.NewGuid())
            .Add(f => f.RequesterMembership, requester)
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(0, 0, 0)));

        var options = cut.FindAll("#invite-layer option").Select(o => o.TextContent);

        options.Should().NotContain("Have Arkitekt");
        options.Should().Contain("Bed Designer");
        options.Should().Contain("Planlægger");
    }

    [Fact]
    public void CategoryDropdown_OnlyShowsOptionsAtOrBelowRequesterCategory()
    {
        var requester = CreateRequester(category: AccessCategory.Editor);
        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, Guid.NewGuid())
            .Add(f => f.RequesterMembership, requester)
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(0, 0, 0)));

        var options = cut.FindAll("#invite-category option").Select(o => o.TextContent);

        options.Should().NotContain("Administrator");
        options.Should().Contain("Redaktør");
    }

    [Fact]
    public async Task EmptyEmail_ClickingSend_ShowsErrorMessage_WithoutCallingService()
    {
        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, Guid.NewGuid())
            .Add(f => f.RequesterMembership, CreateRequester())
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(0, 0, 0)));

        await cut.Find(".form-actions .btn-primary").ClickAsync();

        cut.Find(".status-message.status-danger").Should().NotBeNull();
        await onboardingService.DidNotReceive().CreateInvitationAsync(Arg.Any<CreateInvitationRequestDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ValidEmail_ClickingSend_InvokesOnInvitationCreated_WithResultAndEmail()
    {
        (CreateInvitationResultDto Result, string Email)? invoked = null;
        var gardenId = Guid.NewGuid();
        var expectedResult = new CreateInvitationResultDto(Guid.NewGuid(), "raw-token", DateTimeOffset.UtcNow.AddDays(7), false);

        onboardingService.CreateInvitationAsync(Arg.Any<CreateInvitationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, gardenId)
            .Add(f => f.RequesterMembership, CreateRequester())
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(0, 0, 0))
            .Add(f => f.OnInvitationCreated, r => invoked = r));

        cut.Find("#invite-email").Change("invited@example.com");
        cut.Find(".form-actions .btn-primary").Click();

        invoked.Should().NotBeNull();
        invoked!.Value.Result.Should().Be(expectedResult);
        invoked.Value.Email.Should().Be("invited@example.com");
    }

    [Fact]
    public void ValidEmail_ServiceThrowsInvalidOperationException_ShowsErrorMessage()
    {
        onboardingService.CreateInvitationAsync(Arg.Any<CreateInvitationRequestDto>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Du har ingen ledige gratis invitationer tilbage."));

        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, Guid.NewGuid())
            .Add(f => f.RequesterMembership, CreateRequester())
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(0, 0, 0)));

        cut.Find("#invite-email").Change("invited@example.com");
        cut.Find(".form-actions .btn-primary").Click();

        cut.Markup.Should().Contain("Du har ingen ledige gratis invitationer tilbage.");
    }

    [Fact]
    public void FreeSlotCheckbox_OnlyVisible_WhenQuotaAvailableAndTargetMatchesOwnLayerAndCategory()
    {
        var requester = CreateRequester(layer: GardenAccessLevel.BedDesigner, category: AccessCategory.Editor);
        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, Guid.NewGuid())
            .Add(f => f.RequesterMembership, requester)
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(1, 0, 1)));

        cut.FindAll("#invite-free-slot").Should().HaveCount(1);
    }

    [Fact]
    public void FreeSlotCheckbox_HiddenWhenNoQuotaRemaining()
    {
        var cut = Render<GardenInvitationForm>(p => p
            .Add(f => f.GardenId, Guid.NewGuid())
            .Add(f => f.RequesterMembership, CreateRequester())
            .Add(f => f.FreeQuota, new FreeInvitationQuotaDto(1, 1, 0)));

        cut.FindAll("#invite-free-slot").Should().BeEmpty();
    }
}
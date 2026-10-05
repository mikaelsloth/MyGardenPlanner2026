namespace MyGardenPlanner2026.Tests.UI.Components.Pages.Gardens;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages.Gardens;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public sealed class GardenAccessManagementPageTests : BunitContext
{
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();
    private readonly IOnboardingService onboardingService = Substitute.For<IOnboardingService>();
    private readonly BunitAuthorizationContext authContext;

    public GardenAccessManagementPageTests()
    {
        Services.AddSingleton(queryService);
        Services.AddSingleton(onboardingService);

        authContext = this.AuthorizeAs("user-1");
    }

    private void SetAuthorizationResult(bool succeeded)
    {
        if (succeeded)
        {
            authContext.SetPolicies(AuthorizationServicesExtensions.RequireGardenMemberPolicy);
        }
    }

    /// <summary>
    /// Opsætter "user-1" som ejer (Have Arkitekt / Administrator) af haven, inkl. fri-invitationskvote.
    /// </summary>
    private void ArrangeOwnerOfGarden(Guid gardenId)
    {
        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(GardenTestData.Summary(gardenId));
        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(GardenTestData.Membership(gardenId));
        onboardingService.GetFreeInvitationQuotaAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new FreeInvitationQuotaDto(1, 0, 1));
    }

    [Fact]
    public void NotAMember_ShowsRestrictedEmptyState()
    {
        SetAuthorizationResult(false);

        var cut = Render<GardenAccessManagementPage>(p => p.Add(x => x.GardenId, Guid.NewGuid()));

        cut.Find(".empty-restricted").Should().NotBeNull();
    }

    [Fact]
    public void Member_ShowsGardenNameAndMemberList()
    {
        SetAuthorizationResult(true);
        var gardenId = Guid.NewGuid();

        ArrangeOwnerOfGarden(gardenId);
        queryService.GetMembersAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns([GardenTestData.Membership(gardenId)]);
        queryService.GetInvitationsAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);

        var cut = Render<GardenAccessManagementPage>(p => p.Add(x => x.GardenId, gardenId));

        cut.Markup.Should().Contain("Testhave");
    }

    [Fact]
    public async Task RevokingInvitation_CallsRevokeInvitationAsync()
    {
        SetAuthorizationResult(true);
        var gardenId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();

        ArrangeOwnerOfGarden(gardenId);
        queryService.GetMembersAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        queryService.GetInvitationsAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns([GardenTestData.Invitation(
                invitationId, gardenId, "user-1",
                targetLayer: GardenAccessLevel.HaveArkitekt, targetCategory: AccessCategory.Administrator)]);

        var cut = Render<GardenAccessManagementPage>(p => p.Add(x => x.GardenId, gardenId));

        await cut.Find(".btn-danger").ClickAsync();
        await cut.Find(".confirm-dialog-actions .btn-danger").ClickAsync();

        await onboardingService.Received(1).RevokeInvitationAsync(invitationId, "user-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void CreatingInvitation_ShowsInvitationLinkCard_AndStatusMessage()
    {
        SetAuthorizationResult(true);
        var gardenId = Guid.NewGuid();
        var rawToken = "raw-token-value";

        ArrangeOwnerOfGarden(gardenId);
        queryService.GetMembersAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        queryService.GetInvitationsAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        onboardingService.CreateInvitationAsync(Arg.Any<CreateInvitationRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(new CreateInvitationResultDto(Guid.NewGuid(), rawToken, DateTimeOffset.UtcNow.AddDays(7), false));

        var module = JSInterop.SetupModule("./Components/Domain/Gardens/InvitationLinkCard.razor.js");
        module.SetupVoid("renderQrCode", _ => true).SetVoidResult();

        var cut = Render<GardenAccessManagementPage>(p => p.Add(x => x.GardenId, gardenId));

        cut.Find("#invite-email").Change("invited@example.com");
        cut.Find(".form-actions .btn-primary").Click();

        cut.Markup.Should().Contain("Invitation sendt til invited@example.com");
        cut.Find(".invitation-link-card").Should().NotBeNull();
    }
}
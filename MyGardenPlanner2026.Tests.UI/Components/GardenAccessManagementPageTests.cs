namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Pages.Gardens;
using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using System.Security.Claims;
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

        authContext = AddAuthorization();
        authContext.SetAuthorized("user-1");
        authContext.SetClaims(new Claim(ClaimTypes.NameIdentifier, "user-1"));
    }

    private void SetAuthorizationResult(bool succeeded)
    {
        if (succeeded)
        {
            authContext.SetPolicies(AuthorizationServicesExtensions.RequireGardenMemberPolicy);
        }
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

        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new GardenMembershipDto(Guid.NewGuid(), gardenId, "user-1", true,
                GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, DateTimeOffset.UtcNow));
        queryService.GetMembersAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns([new GardenMembershipDto(Guid.NewGuid(), gardenId, "user-1", true,
                GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, DateTimeOffset.UtcNow)]);
        queryService.GetInvitationsAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        onboardingService.GetFreeInvitationQuotaAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new FreeInvitationQuotaDto(1, 0, 1));

        var cut = Render<GardenAccessManagementPage>(p => p.Add(x => x.GardenId, gardenId));

        cut.Markup.Should().Contain("Testhave");
    }

    [Fact]
    public async Task RevokingInvitation_CallsRevokeInvitationAsync()
    {
        SetAuthorizationResult(true);
        var gardenId = Guid.NewGuid();
        var invitationId = Guid.NewGuid();

        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new GardenMembershipDto(Guid.NewGuid(), gardenId, "user-1", true,
                GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, DateTimeOffset.UtcNow));
        queryService.GetMembersAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        queryService.GetInvitationsAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns([new GardenInvitationDto(invitationId, gardenId, "user-1", "invited@example.com",
                GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, false, false,
                DateTimeOffset.UtcNow.AddDays(7), false, false, DateTimeOffset.UtcNow,
                GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator)]);
        onboardingService.GetFreeInvitationQuotaAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
    .Returns(new FreeInvitationQuotaDto(1, 0, 1));

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

        queryService.GetGardenSummaryAsync(gardenId, Arg.Any<CancellationToken>())
            .Returns(new GardenSummaryDto(gardenId, "Testhave", false));
        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new GardenMembershipDto(Guid.NewGuid(), gardenId, "user-1", true,
                GardenAccessLevel.HaveArkitekt, AccessCategory.Administrator, DateTimeOffset.UtcNow));
        queryService.GetMembersAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        queryService.GetInvitationsAsync(gardenId, Arg.Any<CancellationToken>()).Returns([]);
        onboardingService.GetFreeInvitationQuotaAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new FreeInvitationQuotaDto(1, 0, 1));
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
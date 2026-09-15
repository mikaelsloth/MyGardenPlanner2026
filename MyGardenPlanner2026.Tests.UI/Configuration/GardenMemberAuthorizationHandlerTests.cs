namespace MyGardenPlanner2026.Tests.UI.Configuration;

using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using System.Security.Claims;
using Xunit;

public sealed class GardenMemberAuthorizationHandlerTests
{
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();

    private GardenMemberAuthorizationHandler CreateSut() => new(queryService);

    private static AuthorizationHandlerContext CreateContext(
        IAuthorizationRequirement requirement, ClaimsPrincipal user, Guid resource) =>
        new([requirement], user, resource);

    [Fact]
    public async Task HandleRequirementAsync_UserHasMembership_Succeeds()
    {
        var gardenId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-1")], "TestAuth"));

        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(new GardenMembershipDto(Guid.NewGuid(), gardenId, "user-1", true,
                GardenAccessLevel.BedDesigner, AccessCategory.Editor, DateTimeOffset.UtcNow));

        var context = CreateContext(new GardenMemberRequirement(), user, gardenId);

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasNoMembership_DoesNotSucceed()
    {
        var gardenId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-1")], "TestAuth"));

        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns((GardenMembershipDto?)null);

        var context = CreateContext(new GardenMemberRequirement(), user, gardenId);

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserIdClaim_DoesNotSucceed_AndSkipsQuery()
    {
        var gardenId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        var context = CreateContext(new GardenMemberRequirement(), user, gardenId);

        await CreateSut().HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        await queryService.DidNotReceive().GetMembershipAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
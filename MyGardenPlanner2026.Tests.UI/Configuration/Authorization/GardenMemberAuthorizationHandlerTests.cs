namespace MyGardenPlanner2026.Tests.UI.Configuration.Authorization;

using FluentAssertions;
using MyGardenPlanner2026.Configuration.Authorization;
using MyGardenPlanner2026.Core.Contracts.Onboarding;
using MyGardenPlanner2026.Core.Entities.Common;
using NSubstitute;
using Xunit;

public sealed class GardenMemberAuthorizationHandlerTests
{
    private readonly IGardenAccessQueryService queryService = Substitute.For<IGardenAccessQueryService>();

    private GardenMemberAuthorizationHandler CreateSut() => new(queryService);

    [Fact]
    public async Task HandleRequirementAsync_UserHasMembership_Succeeds()
    {
        var gardenId = Guid.NewGuid();
        var user = TestPrincipals.Create();

        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns(GardenTestData.Membership(gardenId, level: GardenAccessLevel.BedDesigner, category: AccessCategory.Editor));

        var succeeded = await CreateSut().EvaluateAsync(new GardenMemberRequirement(), user, gardenId);

        succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserHasNoMembership_DoesNotSucceed()
    {
        var gardenId = Guid.NewGuid();
        var user = TestPrincipals.Create();

        queryService.GetMembershipAsync(gardenId, "user-1", Arg.Any<CancellationToken>())
            .Returns((GardenMembershipDto?)null);

        var succeeded = await CreateSut().EvaluateAsync(new GardenMemberRequirement(), user, gardenId);

        succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoUserIdClaim_DoesNotSucceed_AndSkipsQuery()
    {
        var gardenId = Guid.NewGuid();
        var user = TestPrincipals.Anonymous();

        var succeeded = await CreateSut().EvaluateAsync(new GardenMemberRequirement(), user, gardenId);

        succeeded.Should().BeFalse();
        await queryService.DidNotReceive().GetMembershipAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
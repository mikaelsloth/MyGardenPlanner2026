namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class ReAuthenticationPolicyAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        using var context = CreateDbContext();
        context.Add(new ReAuthenticationPolicySettings { MaxAgeMinutes = 15 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private (ReAuthenticationPolicyAdminService Service, ISecurityPolicyChangeSignal Signal, ISecurityAlertService AlertService) CreateService()
    {
        var signal = Substitute.For<ISecurityPolicyChangeSignal>();
        var alertService = Substitute.For<ISecurityAlertService>();
        return (new ReAuthenticationPolicyAdminService(CreateAdminDbContextFactory(), signal, alertService), signal, alertService);
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValue()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.MaxAgeMinutes.Should().Be(15);
    }

    [Fact]
    public async Task UpdateAsync_ValidValue_PersistsTriggersAndAlerts()
    {
        await SeedAsync();
        var (service, signal, alertService) = CreateService();

        var result = await service.UpdateAsync(new ReAuthenticationPolicyDto(30), "user-1", TestContext.Current.CancellationToken);

        result.MaxAgeMinutes.Should().Be(30);
        signal.Received(1).TriggerChange<ReAuthenticationPolicyOptions>();
        await alertService.Received(1).AlertPolicyChangedAsync("user-1", "ReAuthenticationPolicy", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_NonPositiveValue_ThrowsArgumentOutOfRangeException()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var act = async () => await service.UpdateAsync(new ReAuthenticationPolicyDto(0), "user-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
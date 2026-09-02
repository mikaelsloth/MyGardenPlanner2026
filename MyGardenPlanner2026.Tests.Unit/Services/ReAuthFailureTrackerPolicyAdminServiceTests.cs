namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class ReAuthFailureTrackerPolicyAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        using var context = CreateDbContext();
        context.Add(new ReAuthFailureTrackerSettings { Threshold = 5, WindowDays = 2 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private (ReAuthFailureTrackerPolicyAdminService Service, ISecurityPolicyChangeSignal Signal, ISecurityAlertService AlertService) CreateService()
    {
        var signal = Substitute.For<ISecurityPolicyChangeSignal>();
        var alertService = Substitute.For<ISecurityAlertService>();
        return (new ReAuthFailureTrackerPolicyAdminService(CreateAdminDbContextFactory(), signal, alertService), signal, alertService);
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValues()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.Threshold.Should().Be(5);
        dto.WindowDays.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_PersistsTriggersAndAlerts()
    {
        await SeedAsync();
        var (service, signal, alertService) = CreateService();

        var result = await service.UpdateAsync(new ReAuthFailureTrackerPolicyDto(10, 7), "user-1", TestContext.Current.CancellationToken);

        result.Threshold.Should().Be(10);
        result.WindowDays.Should().Be(7);
        signal.Received(1).TriggerChange<ReAuthFailureTrackerOptions>();
        await alertService.Received(1).AlertPolicyChangedAsync("user-1", "ReAuthFailureTrackerPolicy", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(5, 0)]
    public async Task UpdateAsync_NonPositiveValues_ThrowsArgumentOutOfRangeException(int threshold, int windowDays)
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var act = async () => await service.UpdateAsync(
            new ReAuthFailureTrackerPolicyDto(threshold, windowDays), "user-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
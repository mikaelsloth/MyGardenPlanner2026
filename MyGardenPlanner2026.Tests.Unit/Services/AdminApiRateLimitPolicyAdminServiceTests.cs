namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class AdminApiRateLimitPolicyAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        using var context = CreateDbContext();
        context.Add(new AdminApiRateLimitSettings { PermitLimit = 100, WindowSeconds = 60, SegmentsPerWindow = 6 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private (AdminApiRateLimitPolicyAdminService Service, ISecurityPolicyChangeSignal Signal, ISecurityAlertService AlertService) CreateService()
    {
        var signal = Substitute.For<ISecurityPolicyChangeSignal>();
        var alertService = Substitute.For<ISecurityAlertService>();
        return (new AdminApiRateLimitPolicyAdminService(CreateAdminDbContextFactory(), signal, alertService), signal, alertService);
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValues()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.PermitLimit.Should().Be(100);
        dto.WindowSeconds.Should().Be(60);
        dto.SegmentsPerWindow.Should().Be(6);
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_PersistsTriggersAndAlerts()
    {
        await SeedAsync();
        var (service, signal, alertService) = CreateService();

        var result = await service.UpdateAsync(new AdminApiRateLimitPolicyDto(200, 30, 3), "user-1", TestContext.Current.CancellationToken);

        result.PermitLimit.Should().Be(200);
        signal.Received(1).TriggerChange<AdminApiRateLimitOptions>();
        await alertService.Received(1).AlertPolicyChangedAsync("user-1", "AdminApiRateLimitPolicy", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 60, 6)]
    [InlineData(100, 0, 6)]
    [InlineData(100, 60, 0)]
    public async Task UpdateAsync_NonPositiveValues_ThrowsArgumentOutOfRangeException(int permit, int window, int segments)
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var act = async () => await service.UpdateAsync(
            new AdminApiRateLimitPolicyDto(permit, window, segments), "user-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
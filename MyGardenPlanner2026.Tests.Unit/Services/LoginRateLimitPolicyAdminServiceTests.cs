namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class LoginRateLimitPolicyAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        using var context = CreateDbContext();
        context.Add(new LoginRateLimitSettings { PermitLimit = 5, WindowSeconds = 60 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private (LoginRateLimitPolicyAdminService Service, ISecurityPolicyChangeSignal Signal, ISecurityAlertService AlertService) CreateService()
    {
        var signal = Substitute.For<ISecurityPolicyChangeSignal>();
        var alertService = Substitute.For<ISecurityAlertService>();
        return (new LoginRateLimitPolicyAdminService(CreateAdminDbContextFactory(), signal, alertService), signal, alertService);
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValues()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.PermitLimit.Should().Be(5);
        dto.WindowSeconds.Should().Be(60);
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_PersistsTriggersAndAlerts()
    {
        await SeedAsync();
        var (service, signal, alertService) = CreateService();

        var result = await service.UpdateAsync(new LoginRateLimitPolicyDto(10, 120), "user-1", TestContext.Current.CancellationToken);

        result.PermitLimit.Should().Be(10);
        result.WindowSeconds.Should().Be(120);
        signal.Received(1).TriggerChange<LoginRateLimitOptions>();
        await alertService.Received(1).AlertPolicyChangedAsync("user-1", "LoginRateLimitPolicy", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 60)]
    [InlineData(5, 0)]
    public async Task UpdateAsync_NonPositiveValues_ThrowsArgumentOutOfRangeException(int permit, int window)
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var act = async () => await service.UpdateAsync(
            new LoginRateLimitPolicyDto(permit, window), "user-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
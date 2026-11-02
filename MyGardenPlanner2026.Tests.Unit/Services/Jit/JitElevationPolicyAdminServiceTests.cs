namespace MyGardenPlanner2026.Tests.Unit.Services.Jit;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Services.Jit;
using NSubstitute;
using Xunit;

public class JitElevationPolicyAdminServiceTests : TestDbContext
{
    private async Task SeedAsync()
    {
        using var context = CreateDbContext();
        context.Add(new JitElevationPolicySettings { MinRequestedMinutes = 30, MaxRequestedMinutes = 90, SweepIntervalMinutes = 5, PendingExpiryHours = 48 });
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private (JitElevationPolicyAdminService Service, ISecurityPolicyChangeSignal Signal, ISecurityAlertService AlertService) CreateService()
    {
        var signal = Substitute.For<ISecurityPolicyChangeSignal>();
        var alertService = Substitute.For<ISecurityAlertService>();
        return (new JitElevationPolicyAdminService(CreateAdminDbContextFactory(), signal, alertService), signal, alertService);
    }

    [Fact]
    public async Task GetAsync_ReturnsSeededValues()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var dto = await service.GetAsync(TestContext.Current.CancellationToken);

        dto.MinRequestedMinutes.Should().Be(30);
        dto.MaxRequestedMinutes.Should().Be(90);
        dto.SweepIntervalMinutes.Should().Be(5);
        dto.PendingExpiryHours.Should().Be(48);
    }

    [Fact]
    public async Task GetAsync_NotSeeded_ThrowsInvalidOperationException()
    {
        var (service, _, _) = CreateService();

        var act = async () => await service.GetAsync(TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_PersistsAndReturnsUpdatedDto()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var result = await service.UpdateAsync(new JitElevationPolicyDto(45, 120, 10, 72), "user-1", TestContext.Current.CancellationToken);

        result.MaxRequestedMinutes.Should().Be(120);
        result.PendingExpiryHours.Should().Be(72);

        var reloaded = await service.GetAsync(TestContext.Current.CancellationToken);
        reloaded.MinRequestedMinutes.Should().Be(45);
        reloaded.SweepIntervalMinutes.Should().Be(10);
        reloaded.PendingExpiryHours.Should().Be(72);
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_TriggersChangeSignalForCorrectOptionsType()
    {
        await SeedAsync();
        var (service, signal, _) = CreateService();

        await service.UpdateAsync(new JitElevationPolicyDto(30, 90, 5, 48), "user-1", TestContext.Current.CancellationToken);

        signal.Received(1).TriggerChange<JitElevationPolicyOptions>();
    }

    [Fact]
    public async Task UpdateAsync_ValidValues_SendsPolicyChangedAlert()
    {
        await SeedAsync();
        var (service, _, alertService) = CreateService();

        await service.UpdateAsync(new JitElevationPolicyDto(30, 90, 5, 48), "user-1", TestContext.Current.CancellationToken);

        await alertService.Received(1).AlertPolicyChangedAsync("user-1", "JitElevationPolicy", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_MinGreaterThanMax_ThrowsArgumentOutOfRangeException()
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var act = async () => await service.UpdateAsync(new JitElevationPolicyDto(100, 90, 5, 48), "user-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(0, 90, 5, 48)]
    [InlineData(30, 0, 5, 48)]
    [InlineData(30, 90, 0, 48)]
    [InlineData(30, 90, 5, 0)]
    [InlineData(30, 90, 5, 721)]
    public async Task UpdateAsync_InvalidValues_ThrowsArgumentOutOfRangeException(int min, int max, int sweep, int pendingExpiry)
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var act = async () => await service.UpdateAsync(
            new JitElevationPolicyDto(min, max, sweep, pendingExpiry), "user-1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(720)]
    public async Task UpdateAsync_PendingExpiryAtBoundaries_Succeeds(int pendingExpiry)
    {
        await SeedAsync();
        var (service, _, _) = CreateService();

        var result = await service.UpdateAsync(
            new JitElevationPolicyDto(30, 90, 5, pendingExpiry), "user-1", TestContext.Current.CancellationToken);

        result.PendingExpiryHours.Should().Be(pendingExpiry);
    }
}
namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Admin;
using MyGardenPlanner2026.Infrastructure.Services;
using NSubstitute;
using Xunit;

public class ReAuthFailureTrackerTests : TestDbContext
{
    private static TestOptionsMonitor<ReAuthFailureTrackerOptions> Policy(int threshold, int windowDays) =>
        new(new ReAuthFailureTrackerOptions { Threshold = threshold, WindowDays = windowDays });

    private (ReAuthFailureTracker Tracker, ISecurityAlertService AlertService, TestTimeProvider TimeProvider) CreateTracker(
        int threshold = 5, int windowDays = 2)
    {
        var alertService = Substitute.For<ISecurityAlertService>();
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero));

        var tracker = new ReAuthFailureTracker(
            CreateAdminDbContextFactory(), Policy(threshold, windowDays), alertService, timeProvider);

        return (tracker, alertService, timeProvider);
    }

    [Fact]
    public async Task RecordFailureAsync_BelowThreshold_ReturnsFalse_AndDoesNotAlert()
    {
        var (tracker, alertService, _) = CreateTracker(threshold: 5);

        var result = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
        await alertService.DidNotReceive().AlertFailedReAuthAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordFailureAsync_ReachesThreshold_ReturnsTrue_AndAlertsExactlyOnce()
    {
        var (tracker, alertService, _) = CreateTracker(threshold: 3);

        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        var thirdResult = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        thirdResult.Should().BeTrue();
        await alertService.Received(1).AlertFailedReAuthAsync("user-1", "10.0.0.1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordFailureAsync_ExceedsThreshold_DoesNotAlertAgain()
    {
        var (tracker, alertService, _) = CreateTracker(threshold: 2);

        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        var fourthResult = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        fourthResult.Should().BeFalse();
        await alertService.Received(1).AlertFailedReAuthAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordFailureAsync_FailuresOutsideWindow_AreNotCounted()
    {
        var (tracker, alertService, timeProvider) = CreateTracker(threshold: 2, windowDays: 2);

        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        timeProvider.Advance(TimeSpan.FromDays(3));

        var result = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        result.Should().BeFalse();
        await alertService.DidNotReceive().AlertFailedReAuthAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordFailureAsync_DifferentUsers_AreTrackedIndependently()
    {
        var (tracker, alertService, _) = CreateTracker(threshold: 1);

        var user1Result = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        var user2Result = await tracker.RecordFailureAsync("user-2", "10.0.0.2", TestContext.Current.CancellationToken);

        user1Result.Should().BeTrue();
        user2Result.Should().BeTrue();
        await alertService.Received(1).AlertFailedReAuthAsync("user-1", "10.0.0.1", Arg.Any<CancellationToken>());
        await alertService.Received(1).AlertFailedReAuthAsync("user-2", "10.0.0.2", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearFailuresAsync_ResetsCounter_SoNextFailureStartsFromOne()
    {
        var (tracker, alertService, _) = CreateTracker(threshold: 2);

        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        await tracker.ClearFailuresAsync("user-1", TestContext.Current.CancellationToken);

        var result = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        result.Should().BeFalse(); // kun 1 forsøg siden nulstilling, tærskel er 2
        await alertService.DidNotReceive().AlertFailedReAuthAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ClearFailuresAsync_NoExistingFailures_DoesNotThrow()
    {
        var (tracker, _, _) = CreateTracker();

        var act = async () => await tracker.ClearFailuresAsync("user-without-failures", TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task RecordFailureAsync_MissingUserId_ThrowsArgumentException(string? userId)
    {
        var (tracker, _, _) = CreateTracker();

        var act = async () => await tracker.RecordFailureAsync(userId!, "10.0.0.1", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task RecordFailureAsync_ThresholdChangedAfterConstruction_UsesUpdatedThresholdImmediately()
    {
        var monitor = new TestOptionsMonitor<ReAuthFailureTrackerOptions>(
            new ReAuthFailureTrackerOptions { Threshold = 5, WindowDays = 2 });
        var alertService = Substitute.For<ISecurityAlertService>();
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 31, 10, 0, 0, TimeSpan.Zero));
        var tracker = new ReAuthFailureTracker(CreateAdminDbContextFactory(), monitor, alertService, timeProvider);

        await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        var secondResult = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);
        secondResult.Should().BeFalse(); // 2 forsøg, gammel tærskel er 5

        monitor.Set(new ReAuthFailureTrackerOptions { Threshold = 3, WindowDays = 2 });

        var thirdResult = await tracker.RecordFailureAsync("user-1", "10.0.0.1", TestContext.Current.CancellationToken);

        thirdResult.Should().BeTrue(); // 3. forsøg rammer den nye, lavere tærskel (3) uden proces-genstart
        await alertService.Received(1).AlertFailedReAuthAsync("user-1", "10.0.0.1", Arg.Any<CancellationToken>());
    }
}
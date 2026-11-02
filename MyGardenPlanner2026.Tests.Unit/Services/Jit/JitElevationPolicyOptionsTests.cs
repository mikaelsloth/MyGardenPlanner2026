namespace MyGardenPlanner2026.Tests.Unit.Services.Jit;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services.Jit;
using Xunit;

public class JitElevationPolicyOptionsTests
{
    [Fact]
    public void DefaultOptions_HasThirtyToNinetyMinuteRange()
    {
        var options = new JitElevationPolicyOptions();

        options.MinRequestedMinutes.Should().Be(30);
        options.MaxRequestedMinutes.Should().Be(90);
    }

    [Fact]
    public void DefaultOptions_HasFiveMinuteSweepInterval()
    {
        var options = new JitElevationPolicyOptions();

        options.SweepIntervalMinutes.Should().Be(5);
    }

    [Fact]
    public void DefaultOptions_HasFortyEightHourPendingExpiry()
    {
        var options = new JitElevationPolicyOptions();

        options.PendingExpiryHours.Should().Be(48);
    }

    [Fact]
    public void PendingExpiry_ReturnsConfiguredHours()
    {
        var options = new JitElevationPolicyOptions { PendingExpiryHours = 72 };

        options.PendingExpiry.Should().Be(TimeSpan.FromHours(72));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void PendingExpiry_NonPositiveHours_FallsBackToOneHour(int hours)
    {
        var options = new JitElevationPolicyOptions { PendingExpiryHours = hours };

        options.PendingExpiry.Should().Be(TimeSpan.FromHours(1));
    }
}
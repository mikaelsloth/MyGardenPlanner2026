namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services;
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
}
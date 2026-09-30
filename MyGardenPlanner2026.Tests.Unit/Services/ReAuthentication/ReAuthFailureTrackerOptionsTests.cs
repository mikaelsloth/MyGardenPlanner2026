namespace MyGardenPlanner2026.Tests.Unit.Services.ReAuthentication;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services.ReAuthentication;
using Xunit;

public class ReAuthFailureTrackerOptionsTests
{
    [Fact]
    public void DefaultOptions_HasThresholdFiveAndWindowTwoDays()
    {
        var options = new ReAuthFailureTrackerOptions();

        options.Threshold.Should().Be(5);
        options.WindowDays.Should().Be(2);
    }
}
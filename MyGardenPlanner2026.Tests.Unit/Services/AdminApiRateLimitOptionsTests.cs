namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class AdminApiRateLimitOptionsTests
{
    [Fact]
    public void DefaultOptions_HasHundredRequestsPerMinute()
    {
        var options = new AdminApiRateLimitOptions();

        options.PermitLimit.Should().Be(100);
        options.WindowSeconds.Should().Be(60);
    }

    [Fact]
    public void DefaultOptions_HasSixSegmentsPerWindow()
    {
        var options = new AdminApiRateLimitOptions();

        options.SegmentsPerWindow.Should().Be(6);
    }
}
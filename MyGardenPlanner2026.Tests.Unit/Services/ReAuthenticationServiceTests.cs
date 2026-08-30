namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class ReAuthenticationServiceTests
{
    [Fact]
    public void IsReAuthValid_NeverMarked_ReturnsFalse()
    {
        var service = new ReAuthenticationService(new TestTimeProvider(DateTimeOffset.UtcNow));

        service.IsReAuthValid(TimeSpan.FromMinutes(15)).Should().BeFalse();
    }

    [Fact]
    public void LastAuthTimestampUtc_NeverMarked_IsNull()
    {
        var service = new ReAuthenticationService(new TestTimeProvider(DateTimeOffset.UtcNow));

        service.LastAuthTimestampUtc.Should().BeNull();
    }

    [Fact]
    public void MarkReAuthenticated_SetsLastAuthTimestampUtc_ToCurrentTime()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero));
        var service = new ReAuthenticationService(timeProvider);

        service.MarkReAuthenticated();

        service.LastAuthTimestampUtc.Should().Be(timeProvider.GetUtcNow());
    }

    [Fact]
    public void IsReAuthValid_JustMarked_ReturnsTrue()
    {
        var timeProvider = new TestTimeProvider(DateTimeOffset.UtcNow);
        var service = new ReAuthenticationService(timeProvider);

        service.MarkReAuthenticated();

        service.IsReAuthValid(TimeSpan.FromMinutes(15)).Should().BeTrue();
    }

    [Fact]
    public void IsReAuthValid_ExactlyAtMaxAge_ReturnsTrue()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero));
        var service = new ReAuthenticationService(timeProvider);
        service.MarkReAuthenticated();

        timeProvider.Advance(TimeSpan.FromMinutes(15));

        service.IsReAuthValid(TimeSpan.FromMinutes(15)).Should().BeTrue();
    }

    [Fact]
    public void IsReAuthValid_OneSecondPastMaxAge_ReturnsFalse()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero));
        var service = new ReAuthenticationService(timeProvider);
        service.MarkReAuthenticated();

        timeProvider.Advance(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        service.IsReAuthValid(TimeSpan.FromMinutes(15)).Should().BeFalse();
    }

    [Fact]
    public void MarkReAuthenticated_CalledAgain_ExtendsValidityWindow()
    {
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2026, 8, 30, 10, 0, 0, TimeSpan.Zero));
        var service = new ReAuthenticationService(timeProvider);
        service.MarkReAuthenticated();

        timeProvider.Advance(TimeSpan.FromMinutes(14));
        service.MarkReAuthenticated();
        timeProvider.Advance(TimeSpan.FromMinutes(10));

        service.IsReAuthValid(TimeSpan.FromMinutes(15)).Should().BeTrue();
    }
}
namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Xunit;

public sealed class TestTimeProviderTests
{
    private static readonly DateTimeOffset Start = new(2026, 9, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GetUtcNow_ReturnsConstructorStartTime()
    {
        var sut = new TestTimeProvider(Start);

        sut.GetUtcNow().Should().Be(Start);
    }

    [Fact]
    public async Task GetUtcNow_DoesNotFollowTheRealClock()
    {
        var sut = new TestTimeProvider(Start);

        await Task.Delay(20, TestContext.Current.CancellationToken);

        sut.GetUtcNow().Should().Be(Start);
    }

    [Fact]
    public void Advance_MovesTimeForwardByDelta()
    {
        var sut = new TestTimeProvider(Start);

        sut.Advance(TimeSpan.FromMinutes(15));

        sut.GetUtcNow().Should().Be(Start.AddMinutes(15));
    }

    [Fact]
    public void SetUtcNow_OverridesCurrentTime()
    {
        var sut = new TestTimeProvider(Start);
        var target = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

        sut.SetUtcNow(target);

        sut.GetUtcNow().Should().Be(target);
    }
}
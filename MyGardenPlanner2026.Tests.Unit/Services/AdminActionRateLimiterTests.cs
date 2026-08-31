namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using Microsoft.Extensions.Options;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class AdminActionRateLimiterTests
{
    private static AdminActionRateLimiter CreateLimiter(int permitLimit = 3, int windowSeconds = 60) =>
        new(Options.Create(new AdminApiRateLimitOptions
        {
            PermitLimit = permitLimit,
            WindowSeconds = windowSeconds,
            SegmentsPerWindow = 1
        }));

    [Fact]
    public async Task TryAcquireAsync_WithinPermitLimit_ReturnsTrue()
    {
        using var limiter = CreateLimiter(permitLimit: 3);

        var result = await limiter.TryAcquireAsync("user-1", TestContext.Current.CancellationToken);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryAcquireAsync_ExceedsPermitLimit_ReturnsFalseOnOverflow()
    {
        using var limiter = CreateLimiter(permitLimit: 2);

        var first = await limiter.TryAcquireAsync("user-1", TestContext.Current.CancellationToken);
        var second = await limiter.TryAcquireAsync("user-1", TestContext.Current.CancellationToken);
        var third = await limiter.TryAcquireAsync("user-1", TestContext.Current.CancellationToken);

        first.Should().BeTrue();
        second.Should().BeTrue();
        third.Should().BeFalse();
    }

    [Fact]
    public async Task TryAcquireAsync_DifferentUsers_HaveIndependentLimits()
    {
        using var limiter = CreateLimiter(permitLimit: 1);

        var user1First = await limiter.TryAcquireAsync("user-1", TestContext.Current.CancellationToken);
        var user1Second = await limiter.TryAcquireAsync("user-1", TestContext.Current.CancellationToken);
        var user2First = await limiter.TryAcquireAsync("user-2", TestContext.Current.CancellationToken);

        user1First.Should().BeTrue();
        user1Second.Should().BeFalse();
        user2First.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TryAcquireAsync_NullOrWhitespaceUserId_ThrowsArgumentException(string? userId)
    {
        using var limiter = CreateLimiter();

        var act = async () => await limiter.TryAcquireAsync(userId!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
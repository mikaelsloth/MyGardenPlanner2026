namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using FluentAssertions;

public sealed class BlazorInteractionTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(20);

    [Fact]
    public async Task ActUntilAsync_OppfyldtVedFoersteForsoeg_KalderHandlingEnGang()
    {
        var calls = 0;

        await BlazorInteraction.ActUntilAsync(
            () => { calls++; return Task.CompletedTask; },
            () => Task.FromResult(true),
            waitPerAttempt: Short);

        calls.Should().Be(1);
    }

    [Fact]
    public async Task ActUntilAsync_OppfyldtEfterToForsoeg_KalderHandlingToGange()
    {
        var calls = 0;

        await BlazorInteraction.ActUntilAsync(
            () => { calls++; return Task.CompletedTask; },
            () => Task.FromResult(calls >= 2),
            waitPerAttempt: Short);

        calls.Should().Be(2);
    }

    [Fact]
    public async Task ActUntilAsync_AldrigOppfyldt_KasterTimeoutEfterMaksForsoeg()
    {
        var calls = 0;

        var act = () => BlazorInteraction.ActUntilAsync(
            () => { calls++; return Task.CompletedTask; },
            () => Task.FromResult(false),
            maxAttempts: 3,
            waitPerAttempt: Short);

        await act.Should().ThrowAsync<TimeoutException>();
        calls.Should().Be(3);
    }

    [Fact]
    public async Task WaitUntilAsync_BetingelseOppfyldtEfterNogleKald_ReturnererTrue()
    {
        var calls = 0;

        var result = await BlazorInteraction.WaitUntilAsync(
            () => Task.FromResult(++calls >= 3), TimeSpan.FromSeconds(2));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WaitUntilAsync_AldrigOppfyldt_ReturnererFalseEfterTimeout()
    {
        var result = await BlazorInteraction.WaitUntilAsync(
            () => Task.FromResult(false), TimeSpan.FromMilliseconds(100));

        result.Should().BeFalse();
    }
}
namespace MyGardenPlanner2026.Tests.Unit.Internal_Tests;

using FluentAssertions;
using Xunit;

public sealed class TestOptionsMonitorTests
{
    private sealed class SampleOptions
    {
        public int Value { get; init; }
    }

    [Fact]
    public void CurrentValue_ReturnsInitialValue()
    {
        var initial = new SampleOptions { Value = 1 };
        var sut = new TestOptionsMonitor<SampleOptions>(initial);

        sut.CurrentValue.Should().BeSameAs(initial);
    }

    [Fact]
    public void Get_ReturnsCurrentValue_RegardlessOfName()
    {
        var sut = new TestOptionsMonitor<SampleOptions>(new SampleOptions { Value = 1 });

        sut.Get("anything").Should().BeSameAs(sut.CurrentValue);
    }

    [Fact]
    public void Set_UpdatesCurrentValue()
    {
        var sut = new TestOptionsMonitor<SampleOptions>(new SampleOptions { Value = 1 });
        var updated = new SampleOptions { Value = 2 };

        sut.Set(updated);

        sut.CurrentValue.Should().BeSameAs(updated);
    }

    [Fact]
    public void Set_NotifiesAllRegisteredListeners_WithNewValue()
    {
        var sut = new TestOptionsMonitor<SampleOptions>(new SampleOptions { Value = 1 });
        var received = new List<int>();
        using var first = sut.OnChange((o, _) => received.Add(o.Value));
        using var second = sut.OnChange((o, _) => received.Add(o.Value * 10));

        sut.Set(new SampleOptions { Value = 2 });

        received.Should().BeEquivalentTo([2, 20]);
    }

    [Fact]
    public void Set_AfterListenerDisposed_DoesNotNotifyThatListener()
    {
        var sut = new TestOptionsMonitor<SampleOptions>(new SampleOptions { Value = 1 });
        var calls = 0;
        var subscription = sut.OnChange((_, _) => calls++);

        subscription.Dispose();
        sut.Set(new SampleOptions { Value = 2 });

        calls.Should().Be(0);
    }
}
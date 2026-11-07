namespace MyGardenPlanner2026.Tests.UI.Components.Foundation;

using FluentAssertions;
using Microsoft.AspNetCore.Components;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public class FocusTrapScopeTests
{
    private static readonly ElementReference Container = default!;

    [Fact]
    public async Task SyncAsync_WhenInactive_DoesNothing()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);

        await scope.SyncAsync(false, Container);

        service.ActivateCount.Should().Be(0);
    }

    [Fact]
    public async Task SyncAsync_WhenActivated_ActivatesTrap()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);

        await scope.SyncAsync(true, Container);

        service.ActivateCount.Should().Be(1);
        service.DisposedCount.Should().Be(0);
    }

    [Fact]
    public async Task SyncAsync_SameStateTwice_ActivatesOnce()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);

        await scope.SyncAsync(true, Container);
        await scope.SyncAsync(true, Container);

        service.ActivateCount.Should().Be(1);
    }

    [Fact]
    public async Task SyncAsync_ActiveThenInactive_ReleasesTrap()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);

        await scope.SyncAsync(true, Container);
        await scope.SyncAsync(false, Container);

        service.DisposedCount.Should().Be(1);
    }

    [Fact]
    public async Task SyncAsync_ReopenedAfterClose_ActivatesAgain()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);

        await scope.SyncAsync(true, Container);
        await scope.SyncAsync(false, Container);
        await scope.SyncAsync(true, Container);

        service.ActivateCount.Should().Be(2);
        service.DisposedCount.Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsync_WhileActive_ReleasesTrap()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);
        await scope.SyncAsync(true, Container);

        await scope.DisposeAsync();

        service.DisposedCount.Should().Be(1);
    }

    [Fact]
    public async Task DisposeAsync_WithoutActivation_DoesNothing()
    {
        var service = new FakeFocusTrapService();
        var scope = new FocusTrapScope(service);

        await scope.DisposeAsync();

        service.ActivateCount.Should().Be(0);
        service.DisposedCount.Should().Be(0);
    }

    [Fact]
    public async Task SyncAsync_DeactivatedWhileActivationPending_DisposesLateHandle()
    {
        var service = new GatedFocusTrapService();
        var scope = new FocusTrapScope(service);

        var first = scope.SyncAsync(true, Container);
        await scope.SyncAsync(false, Container);
        var late = new TestHandle();
        service.Complete(0, late);
        await first;

        late.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task SyncAsync_ReactivatedWhileFirstActivationPending_DisposesStaleHandle()
    {
        var service = new GatedFocusTrapService();
        var scope = new FocusTrapScope(service);

        var first = scope.SyncAsync(true, Container);
        await scope.SyncAsync(false, Container);
        var second = scope.SyncAsync(true, Container);

        var stale = new TestHandle();
        var current = new TestHandle();

        service.Complete(0, stale);
        await first;
        stale.IsDisposed.Should().BeTrue();

        service.Complete(1, current);
        await second;
        current.IsDisposed.Should().BeFalse();

        await scope.DisposeAsync();
        current.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_WhileActivationPending_DisposesLateHandle()
    {
        var service = new GatedFocusTrapService();
        var scope = new FocusTrapScope(service);

        var first = scope.SyncAsync(true, Container);
        await scope.DisposeAsync();
        var late = new TestHandle();
        service.Complete(0, late);
        await first;

        late.IsDisposed.Should().BeTrue();
    }

    /// <summary>Hver aktivering afventer, til testen kalder Complete(index, handle).</summary>
    private sealed class GatedFocusTrapService : IFocusTrapService
    {
        private readonly List<TaskCompletionSource<IAsyncDisposable>> gates = [];

        public ValueTask<IAsyncDisposable> ActivateAsync(ElementReference container)
        {
            var gate = new TaskCompletionSource<IAsyncDisposable>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            gates.Add(gate);
            return new ValueTask<IAsyncDisposable>(gate.Task);
        }

        public void Complete(int index, IAsyncDisposable handle) => gates[index].SetResult(handle);
    }

    private sealed class TestHandle : IAsyncDisposable
    {
        public bool IsDisposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Foundation;

/// <summary>
/// Test-double til IFocusTrapService. Tæller aktiveringer og frigivelser, uden JS-interop.
/// </summary>
public sealed class FakeFocusTrapService : IFocusTrapService
{
    private readonly List<Handle> handles = [];

    public int ActivateCount => handles.Count;

    public int DisposedCount => handles.Count(h => h.IsDisposed);

    public ValueTask<IAsyncDisposable> ActivateAsync(ElementReference container)
    {
        var handle = new Handle();
        handles.Add(handle);
        return ValueTask.FromResult<IAsyncDisposable>(handle);
    }

    private sealed class Handle : IAsyncDisposable
    {
        public bool IsDisposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return ValueTask.CompletedTask;
        }
    }
}

public static class FocusTrapTestExtensions
{
    /// <summary>Registrerer en FakeFocusTrapService og returnerer den, så tests kan aflæse tællerne.</summary>
    public static FakeFocusTrapService AddFakeFocusTrap(this BunitContext context)
    {
        var fake = new FakeFocusTrapService();
        context.Services.AddSingleton<IFocusTrapService>(fake);
        return fake;
    }
}
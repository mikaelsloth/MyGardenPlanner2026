namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Scoped (én pr. circuit). Importerer JS-modulet første gang et trap aktiveres.
/// </summary>
public sealed class FocusTrapService(IJSRuntime js) : IFocusTrapService, IAsyncDisposable
{
    private const string ModulePath = "./js/focus-trap.js";

    private readonly SemaphoreSlim moduleLock = new(1, 1);
    private IJSObjectReference? module;

    public async ValueTask<IAsyncDisposable> ActivateAsync(ElementReference container)
    {
        var jsModule = await GetModuleAsync();
        var id = await jsModule.InvokeAsync<int>("activate", container);

        return new Handle(jsModule, id);
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        await moduleLock.WaitAsync();
        try
        {
            module ??= await js.InvokeAsync<IJSObjectReference>("import", ModulePath);
            return module;
        }
        finally
        {
            moduleLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit already disconnected — intet at rydde op på klientsiden.
        }

        moduleLock.Dispose();
    }

    private sealed class Handle(IJSObjectReference module, int id) : IAsyncDisposable
    {
        private bool disposed;

        public async ValueTask DisposeAsync()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;

            try
            {
                await module.InvokeVoidAsync("deactivate", id);
            }
            catch (JSDisconnectedException)
            {
                // Circuit already disconnected — intet at rydde op på klientsiden.
            }
        }
    }
}
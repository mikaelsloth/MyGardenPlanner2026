namespace MyGardenPlanner2026.Tests.UI.Internal_Tests;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MyGardenPlanner2026.Tests.UI.Components.Layout;
using Xunit;

public sealed class NavDrawerJsInteropTests : BunitContext
{
    [Fact]
    public async Task SetupNavDrawerModule_ModuleAcceptsActivateAndDeactivate()
    {
        var setup = this.SetupNavDrawerModule();
        var runtime = Services.GetRequiredService<IJSRuntime>();

        await using var module = await runtime.InvokeAsync<IJSObjectReference>("import", NavDrawerJsInterop.ModulePath);
        await module.InvokeVoidAsync("activate", "drawer");
        await module.InvokeVoidAsync("deactivate");

        setup.VerifyInvoke("activate");
        setup.VerifyInvoke("deactivate");
    }
}
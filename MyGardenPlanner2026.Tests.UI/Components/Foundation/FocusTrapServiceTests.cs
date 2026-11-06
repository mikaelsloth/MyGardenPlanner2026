namespace MyGardenPlanner2026.Tests.UI.Components.Foundation;

using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MyGardenPlanner2026.Components.Foundation;
using Xunit;

public sealed class FocusTrapServiceTests : BunitContext
{
    private const string ModulePath = "./js/focus-trap.js";

    private BunitJSModuleInterop SetupModule(int activationId = 1)
    {
        var module = JSInterop.SetupModule(ModulePath);
        module.Setup<int>("activate", _ => true).SetResult(activationId);
        return module;
    }

    private FocusTrapService CreateService() =>
        new(Services.GetRequiredService<IJSRuntime>());

    [Fact]
    public async Task ActivateAsync_CalledTwice_ImportsModuleOnce()
    {
        var module = SetupModule();
        var service = CreateService();

        await service.ActivateAsync(new ElementReference("a"));
        await service.ActivateAsync(new ElementReference("b"));

        JSInterop.Invocations.Count(i => i.Identifier == "import").Should().Be(1);
        module.VerifyInvoke("activate", calledTimes: 2);
    }

    [Fact]
    public async Task HandleDispose_InvokesDeactivateWithActivationId()
    {
        var module = SetupModule(activationId: 7);
        module.SetupVoid("deactivate",
            invocation => invocation.Arguments.Count == 1 && invocation.Arguments[0] is int id && id == 7)
            .SetVoidResult();
        var service = CreateService();

        var handle = await service.ActivateAsync(new ElementReference("a"));
        await handle.DisposeAsync();

        module.VerifyInvoke("deactivate");
    }

    [Fact]
    public async Task HandleDispose_CalledTwice_InvokesDeactivateOnce()
    {
        var module = SetupModule();
        module.SetupVoid("deactivate", _ => true).SetVoidResult();
        var service = CreateService();

        var handle = await service.ActivateAsync(new ElementReference("a"));
        await handle.DisposeAsync();
        await handle.DisposeAsync();

        module.VerifyInvoke("deactivate", calledTimes: 1);
    }

    [Fact]
    public async Task HandleDispose_WhenCircuitDisconnected_DoesNotThrow()
    {
        var module = SetupModule();
        module.SetupVoid("deactivate", _ => true)
            .SetException(new JSDisconnectedException("Circuit disconnected"));
        var service = CreateService();
        var handle = await service.ActivateAsync(new ElementReference("a"));

        var act = async () => await handle.DisposeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ServiceDispose_WithoutActivation_DoesNotImportModule()
    {
        SetupModule();
        var service = CreateService();

        await service.DisposeAsync();

        JSInterop.Invocations.Should().NotContain(i => i.Identifier == "import");
    }
}
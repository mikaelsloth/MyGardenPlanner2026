namespace MyGardenPlanner2026.Tests.UI.Components;

using Bunit;
using FluentAssertions;
using MyGardenPlanner2026.Components.Layout;
using Xunit;

public class NavDrawerFocusTrapTests : BunitContext
{
    [Fact]
    public void NavDrawer_OnFirstRender_ImportsFocusTrapModule()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();

        Render<NavDrawer>(p => p.Add(x => x.IsOpen, false));

        JSInterop.Invocations.Should().Contain(i => i.Identifier == "import");
    }

    [Fact]
    public void NavDrawer_WhenOpening_InvokesActivateWithDrawerElement()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();

        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, false));

        cut.Render(p => p.Add(x => x.IsOpen, true));

        cut.WaitForAssertion(() => module.VerifyInvoke("activate"));
    }

    [Fact]
    public void NavDrawer_WhenClosing_InvokesDeactivate()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();

        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, true));
        cut.WaitForAssertion(() => module.VerifyInvoke("activate"));

        cut.Render(p => p.Add(x => x.IsOpen, false));

        cut.WaitForAssertion(() => module.VerifyInvoke("deactivate"));
    }

    [Fact]
    public void NavDrawer_ReopeningAfterClose_InvokesActivateTwice()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();

        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, false));

        cut.Render(p => p.Add(x => x.IsOpen, true));
        cut.WaitForAssertion(() => module.VerifyInvoke("activate"));

        cut.Render(p => p.Add(x => x.IsOpen, false));
        cut.WaitForAssertion(() => module.VerifyInvoke("deactivate"));

        cut.Render(p => p.Add(x => x.IsOpen, true));

        cut.WaitForAssertion(() => module.VerifyInvoke("activate", calledTimes: 2));
    }

    [Fact]
    public async Task NavDrawer_DisposedWhileOpen_InvokesDeactivateDuringCleanup()
    {
        var module = JSInterop.SetupModule("./Components/Layout/NavDrawer.razor.js");
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();

        var cut = Render<NavDrawer>(p => p.Add(x => x.IsOpen, true));
        cut.WaitForAssertion(() => module.VerifyInvoke("activate"));

        await cut.Instance.DisposeAsync();

        module.VerifyInvoke("deactivate");
    }
}
namespace MyGardenPlanner2026.Tests.UI.Components.Layout;

using Bunit;

/// <summary>
/// Fælles bUnit JS-interop opsætning for NavDrawers focus-trap modul.
/// </summary>
public static class NavDrawerJsInterop
{
    public const string ModulePath = "./Components/Layout/NavDrawer.razor.js";

    /// <summary>
    /// Sætter modulet op, så activate(...) og deactivate() accepteres.
    /// Returnerer modulet, så tests kan verificere kald med VerifyInvoke.
    /// </summary>
    public static BunitJSModuleInterop SetupNavDrawerModule(this BunitContext context)
    {
        var module = context.JSInterop.SetupModule(ModulePath);
        module.SetupVoid("activate", _ => true).SetVoidResult();
        module.SetupVoid("deactivate").SetVoidResult();
        return module;
    }
}
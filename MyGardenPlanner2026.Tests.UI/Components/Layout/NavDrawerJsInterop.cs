namespace MyGardenPlanner2026.Tests.UI.Components.Layout;

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MyGardenPlanner2026.Components.Foundation;

/// <summary>
/// Fælles bUnit-opsætning for NavDrawers fokus-trap: registrerer den rigtige FocusTrapService
/// og sætter det delte JS-modul op, så activate(...) og deactivate(...) accepteres.
/// Returnerer modulet, så tests kan verificere kald med VerifyInvoke.
/// </summary>
public static class NavDrawerJsInterop
{
    public const string ModulePath = "./js/focus-trap.js";

    public static BunitJSModuleInterop SetupNavDrawerModule(this BunitContext context)
    {
        context.Services.AddScoped<IFocusTrapService, FocusTrapService>();

        var module = context.JSInterop.SetupModule(ModulePath);
        module.Setup<int>("activate", _ => true).SetResult(1);
        module.SetupVoid("deactivate", _ => true).SetVoidResult();
        return module;
    }
}
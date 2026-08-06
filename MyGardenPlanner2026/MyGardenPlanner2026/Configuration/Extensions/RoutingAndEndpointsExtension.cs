namespace MyGardenPlanner2026.Configuration.Extensions;

using MyGardenPlanner2026.Components;

public static class RoutingAndEndpointsExtension
{
    public static WebApplication MapRoutingEndpoints(this WebApplication app)
    {
        app.MapStaticAssets();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(MyGardenPlanner2026.Client._Imports).Assembly);

        app.MapAdditionalIdentityEndpoints();
        return app;
    }
}
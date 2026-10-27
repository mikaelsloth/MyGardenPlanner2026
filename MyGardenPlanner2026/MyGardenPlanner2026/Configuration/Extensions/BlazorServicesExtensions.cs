namespace MyGardenPlanner2026.Configuration.Extensions;

using Microsoft.AspNetCore.Components.Server.Circuits;
using MyGardenPlanner2026.Core.Contracts.Common;
using MyGardenPlanner2026.Services;

public static class BlazorServicesExtensions
{
    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddScoped<CircuitHandler, CircuitUserCircuitHandler>();

        return services;
    }
}
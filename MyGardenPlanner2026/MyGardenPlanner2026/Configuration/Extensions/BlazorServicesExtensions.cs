namespace MyGardenPlanner2026.Configuration.Extensions;

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

        return services;
    }
}
namespace MyGardenPlanner2026.Configuration.Extensions;

public static class AuthorizationServicesExtensions
{
    public const string RequireGlobalAdminPolicy = "RequireGlobalAdmin";
    public const string SystemAdminRole = "SystemAdmin";

    public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(RequireGlobalAdminPolicy, policy => policy.RequireRole(SystemAdminRole));

        return services;
    }
}
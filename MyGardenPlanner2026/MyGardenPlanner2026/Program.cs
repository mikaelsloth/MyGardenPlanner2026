using MyGardenPlanner2026.Configuration.Extensions;
using MyGardenPlanner2026.Core.Contracts.Admin;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Configuration["DatabaseProvider"];

// 1. Registrer services via modulariserede extension-metoder
builder.Services
    .AddBlazorServices()
    .AddDatabaseServices(builder.Configuration, provider)
    .AddIdentityServices()
    .AddIdentityBootstrapSeeding(builder.Configuration)
    .AddAuthorizationServices()
    .AddSubscriptionCatalogServices()
    .AddJitElevationServices(builder.Configuration)
    .AddReAuthenticationServices(builder.Configuration)
    .AddRateLimitingServices()
    .AddAdminApiRateLimiting(builder.Configuration)
    .AddSecurityAlertingServices(builder.Configuration)
    .AddReAuthFailureTracking(builder.Configuration)
    .AddSecurityPolicySettingsSeeding(builder.Configuration)
    .AddSecurityPolicyRuntimeReload();

var app = builder.Build();

#if DEBUG
using var scope = app.Services.CreateScope();
var service = scope.ServiceProvider.GetRequiredService<ISecurityAlertService>();
await service.AlertPolicyChangedAsync("testuser", "loginpolicy", CancellationToken.None);
#endif

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"DatabaseProvider: {provider}");

// 1.1 Add seeds in development
if (app.Environment.IsDevelopment())
{
    await app.Services.AddDatabaseSeedsAsync();
}

// 1.2 Bootstrap SystemAdmin-rolle og -bruger (Development OG Production)
await app.Services.SeedIdentityBootstrapAsync();

// 1.3 Seed sikkerhedspolicy-indstillinger (Development OG Production)
await app.Services.SeedSecurityPolicySettingsAsync();

// 2. HTTP Request Pipeline (Middleware)
app.UseWebPipeline();

// 3. Routing & Endpoints
app.MapRoutingEndpoints();

app.Run();
using MyGardenPlanner2026.Configuration.Extensions;

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
    .AddAdminApiRateLimiting(builder.Configuration);

var app = builder.Build();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"DatabaseProvider: {provider}");

// 1.1 Add seeds in development
if (app.Environment.IsDevelopment())
    await app.Services.AddDatabaseSeeds();

// 1.2 Bootstrap SystemAdmin-rolle og -bruger (Development OG Production)
await app.Services.SeedIdentityBootstrapAsync();

// 2. HTTP Request Pipeline (Middleware)
app.UseWebPipeline();

// 3. Routing & Endpoints
app.MapRoutingEndpoints();

app.Run();
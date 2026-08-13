using MyGardenPlanner2026.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Configuration["DatabaseProvider"];

// 1. Registrer services via modulariserede extension-metoder
builder.Services
    .AddBlazorServices()
    .AddDatabaseServices(builder.Configuration, provider)
    .AddIdentityServices()
    .AddSubscriptionCatalogServices();

var app = builder.Build();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"DatabaseProvider: {provider}");

// 1.1 Add seeds in development
if (app.Environment.IsDevelopment())
    await app.Services.AddDatabaseSeeds();

// 2. HTTP Request Pipeline (Middleware)
app.UseWebPipeline();

// 3. Routing & Endpoints
app.MapRoutingEndpoints();

app.Run();
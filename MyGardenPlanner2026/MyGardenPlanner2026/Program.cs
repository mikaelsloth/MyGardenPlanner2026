using MyGardenPlanner2026.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);
var provider = builder.Configuration["DatabaseProvider"];

// 1. Registrer services via modulariserede extension-metoder
builder.Services
    .AddBlazorServices()
    .AddDatabaseServices(builder.Configuration, provider)
    .AddIdentityServices();

var app = builder.Build();

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"DatabaseProvider: {provider}");

// 2. HTTP Request Pipeline (Middleware)
app.UseWebPipeline();

// 3. Routing & Endpoints
app.MapRoutingEndpoints();

app.Run();
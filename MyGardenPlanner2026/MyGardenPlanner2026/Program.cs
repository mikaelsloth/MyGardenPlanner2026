using MyGardenPlanner2026.Configuration.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrer services via modulariserede extension-metoder
builder.Services
    .AddBlazorServices()
    .AddDatabaseServices(builder.Configuration)
    .AddIdentityServices();

var app = builder.Build();

// 2. HTTP Request Pipeline (Middleware)
app.UseWebPipeline();

// 3. Routing & Endpoints
app.MapRoutingEndpoints();

app.Run();
namespace MyGardenPlanner2026.Configuration.Extensions;

public static class PipelineConfigurationExtensions
{
    public static WebApplication UseWebPipeline(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        app.UseRateLimiter();
        app.UseAntiforgery();

        return app;
    }
}
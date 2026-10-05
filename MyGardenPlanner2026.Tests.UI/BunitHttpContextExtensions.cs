namespace MyGardenPlanner2026.Tests.UI;

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Render-hjælper til komponenter, der kræver en cascading HttpContext (statisk SSR / Identity-sider).
/// </summary>
public static class BunitHttpContextExtensions
{
    /// <summary>
    /// Renderer <typeparamref name="TComponent"/> med en cascading HttpContext.
    /// </summary>
    /// <param name="httpContext">Udeladt oprettes en ny DefaultHttpContext.</param>
    /// <param name="parameterBuilder">Ekstra komponentparametre.</param>
    public static IRenderedComponent<TComponent> RenderWithHttpContext<TComponent>(
        this BunitContext context,
        DefaultHttpContext? httpContext = null,
        Action<ComponentParameterCollectionBuilder<TComponent>>? parameterBuilder = null)
        where TComponent : IComponent
    {
        var cascadingHttpContext = httpContext ?? new DefaultHttpContext();

        return context.Render<TComponent>(parameters =>
        {
            parameters.AddCascadingValue(cascadingHttpContext);
            parameterBuilder?.Invoke(parameters);
        });
    }
}
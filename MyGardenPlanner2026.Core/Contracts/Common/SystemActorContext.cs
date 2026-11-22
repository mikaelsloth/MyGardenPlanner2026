namespace MyGardenPlanner2026.Core.Contracts.Common;

using System.Runtime.CompilerServices;

/// <summary>
/// Ambient systemaktør for handlinger uden bruger (baggrundstjenester, seedere). Bruges af
/// SoftDeleteInterceptor, når ICurrentUserAccessor ikke har en bruger. Brug:
/// <c>using var scope = SystemActorContext.Begin&lt;MinKlasse&gt;();</c> (metodenavnet udledes).
/// </summary>
public static class SystemActorContext
{
    public const string UnknownActor = "system-unknown";

    private static readonly AsyncLocal<string?> CurrentValue = new();

    public static string? Current => CurrentValue.Value;

    public static IDisposable Begin<T>([CallerMemberName] string method = "") =>
        Begin(typeof(T).Name, method);

    /// <summary>Til statiske klasser, der ikke kan bruges som typeargument.</summary>
    public static IDisposable Begin(string className, string method)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(className);
        ArgumentException.ThrowIfNullOrWhiteSpace(method);

        var previous = CurrentValue.Value;
        CurrentValue.Value = $"system-{className}.{method}";
        return new Scope(previous);
    }

    private sealed class Scope(string? previous) : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            CurrentValue.Value = previous;
        }
    }
}
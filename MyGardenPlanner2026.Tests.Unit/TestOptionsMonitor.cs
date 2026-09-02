namespace MyGardenPlanner2026.Tests.Unit;

using Microsoft.Extensions.Options;

/// <summary>
/// Minimal, manuelt styret IOptionsMonitor&lt;T&gt; til unit-tests af services, der
/// reagerer på runtime-ændringer af sikkerhedspolicies (IOptionsMonitor.OnChange).
/// CurrentValue kan sættes direkte via Set(...), som samtidig udløser alle
/// registrerede OnChange-listeners — samme kontrakt som den rigtige
/// IOptionsMonitorCache-baserede implementering, men uden den fulde Options-infrastruktur.
/// </summary>
public sealed class TestOptionsMonitor<T>(T currentValue) : IOptionsMonitor<T> where T : class
{
    private readonly List<Action<T, string?>> _listeners = [];

    public T CurrentValue { get; private set; } = currentValue;

    public T Get(string? name) => CurrentValue;

    public IDisposable OnChange(Action<T, string?> listener)
    {
        _listeners.Add(listener);
        return new Unsubscriber(() => _listeners.Remove(listener));
    }

    public void Set(T newValue)
    {
        CurrentValue = newValue;
        foreach (var listener in _listeners.ToArray())
        {
            listener(newValue, null);
        }
    }

    private sealed class Unsubscriber(Action unsubscribe) : IDisposable
    {
        public void Dispose() => unsubscribe();
    }
}
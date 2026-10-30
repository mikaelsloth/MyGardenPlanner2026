namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>
/// Skriver appens output til en fil med UTC-tidsstempel pr. linje. Trådsikker, da stdout og
/// stderr leveres fra forskellige tråde. Skrivning efter Dispose ignoreres.
/// </summary>
internal sealed class AppLogFile : IDisposable
{
    private readonly Lock _lock = new();
    private readonly Func<DateTime> _utcNow;
    private StreamWriter? _writer;

    public AppLogFile(string path, Func<DateTime>? utcNow = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        _writer = new StreamWriter(path, append: false) { AutoFlush = true };
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    public void WriteLine(string line)
    {
        lock (_lock)
        {
            _writer?.WriteLine($"{_utcNow():HH:mm:ss.fff} {line}");
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _writer?.Dispose();
            _writer = null;
        }
    }
}
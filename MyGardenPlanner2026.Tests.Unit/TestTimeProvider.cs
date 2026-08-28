namespace MyGardenPlanner2026.Tests.Unit;

/// <summary>
/// Minimal, manuelt styret TimeProvider til unit-tests af tidsafhængig logik
/// (fx udløbsgrænser). Starter ved en fast konstruktør-angivet tid og rykkes
/// kun fremad eksplicit via Advance/SetUtcNow — aldrig af det reelle ur.
/// </summary>
public sealed class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public TestTimeProvider(DateTimeOffset start) => _utcNow = start;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan delta) => _utcNow += delta;

    public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow;
}
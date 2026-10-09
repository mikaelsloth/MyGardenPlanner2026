namespace MyGardenPlanner2026.Tests.E2E;

/// <summary>
/// Blazor Server ignorerer brugerhændelser, indtil kredsløbet er forbundet. ActUntilAsync
/// gentager handlingen, indtil den forventede tilstand er nået, så testen ikke afhænger af
/// timing ved sideindlæsning. Bruges kun til handlinger, der ikke skader at gentage, mens
/// tilstanden endnu ikke er nået (fx åbn-knap).
/// </summary>
public static class BlazorInteraction
{
    public static async Task ActUntilAsync(
        Func<Task> action,
        Func<Task<bool>> reached,
        int maxAttempts = 8,
        TimeSpan? waitPerAttempt = null,
        string? description = null)
    {
        var wait = waitPerAttempt ?? TimeSpan.FromMilliseconds(750);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await action();

            if (await WaitUntilAsync(reached, wait))
            {
                return;
            }
        }

        throw new TimeoutException(
            $"{description ?? "Forventet tilstand"} blev ikke nået efter {maxAttempts} forsøg.");
    }

    /// <summary>Poller betingelsen hvert 50 ms og returnerer, om den blev opfyldt inden timeout.</summary>
    public static async Task<bool> WaitUntilAsync(Func<Task<bool>> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (true)
        {
            if (await condition())
            {
                return true;
            }

            if (DateTime.UtcNow >= deadline)
            {
                return false;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }
    }
}
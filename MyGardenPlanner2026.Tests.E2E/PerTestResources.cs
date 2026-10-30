namespace MyGardenPlanner2026.Tests.E2E;

using Microsoft.Playwright;

/// <summary>
/// Holder ressourcer (fx browser-kontekster) og lukker dem, når en ny test begynder.
/// Forudsætter sekventiel kørsel (AssemblyInfo: ParallelMode.None). Uden dette hober
/// åbne sider og Blazor-circuits sig op gennem hele kørslen.
/// </summary>
internal sealed class PerTestResources<T>(Func<string?> currentTestId, Func<T, Task> close)
{
    private readonly List<T> _items = [];
    private string? _ownerTestId;

    public int Count => _items.Count;

    /// <summary>Kaldes før en ny ressource oprettes. Lukker alt fra tidligere tests.</summary>
    public async Task ReleaseIfNewTestAsync()
    {
        var id = currentTestId();
        if (id is null || id == _ownerTestId)
        {
            return;
        }

        _ownerTestId = id;
        await CloseAllAsync();
    }

    public void Add(T item) => _items.Add(item);

    public async Task CloseAllAsync()
    {
        var items = _items.ToArray();
        _items.Clear();

        foreach (var item in items)
        {
            try
            {
                await close(item);
            }
            catch (PlaywrightException)
            {
                // allerede lukket eller browseren er væk: intet at rydde op
            }
        }
    }
}
namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Fanger tastaturfokus i en container (drawer, dialog). Kan mockes i bUnit.
/// </summary>
public interface IFocusTrapService
{
    /// <summary>
    /// Aktiverer et fokus-trap i <paramref name="container"/>. Containeren skal være renderet.
    /// Det returnerede handle frigiver trap'et og returnerer fokus til elementet, der havde det før (idempotent).
    /// Initialt fokus: elementet med data-autofocus, ellers det første fokuserbare element.
    /// </summary>
    ValueTask<IAsyncDisposable> ActivateAsync(ElementReference container);
}
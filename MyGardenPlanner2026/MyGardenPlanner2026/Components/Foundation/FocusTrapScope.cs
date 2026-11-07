namespace MyGardenPlanner2026.Components.Foundation;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Styrer livscyklussen for ét fokus-trap i en komponent (drawer, dialog).
/// Kald <see cref="SyncAsync"/> fra OnAfterRenderAsync med komponentens "er åben"-tilstand og
/// containerens ElementReference, og <see cref="DisposeAsync"/> fra komponentens egen DisposeAsync.
///
/// Bevidst IKKE en Blazor-komponent eller DI-registreret service — en almindelig klasse ejet som
/// felt af komponenten, så tilstanden er isoleret pr. komponent-instans.
/// </summary>
public sealed class FocusTrapScope(IFocusTrapService focusTrapService) : IAsyncDisposable
{
    private bool isActive;
    private int version;
    private IAsyncDisposable? trap;

    /// <summary>
    /// Aktiverer eller frigiver trap'et, så det matcher <paramref name="shouldBeActive"/>.
    /// Gør ingenting, hvis tilstanden er uændret.
    /// </summary>
    public async Task SyncAsync(bool shouldBeActive, ElementReference container)
    {
        if (shouldBeActive == isActive)
        {
            return;
        }

        isActive = shouldBeActive;
        var myVersion = ++version;

        if (!shouldBeActive)
        {
            await ReleaseAsync();
            return;
        }

        var activated = await focusTrapService.ActivateAsync(container);

        // Tilstanden er ændret (lukket, genåbnet eller disposed), mens aktiveringen afventede.
        if (myVersion == version)
        {
            trap = activated;
        }
        else
        {
            await activated.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        isActive = false;
        version++;
        await ReleaseAsync();
    }

    private async ValueTask ReleaseAsync()
    {
        var current = trap;
        trap = null;

        if (current is not null)
        {
            await current.DisposeAsync();
        }
    }
}
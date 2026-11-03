namespace MyGardenPlanner2026.Tests.Unit.Services.Layer1;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>Kaster en DbUpdateException, der ikke er et duplikat, ved første SaveChanges.</summary>
internal sealed class ThrowingSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) =>
        throw new DbUpdateException("Simuleret databasefejl.");
}

/// <summary>
/// Kører en handling lige før første SaveChanges. Bruges til at simulere en race:
/// en anden bruger indsætter den konkurrerende række, efter servicens forkontrol er bestået.
/// </summary>
internal sealed class BeforeFirstSaveInterceptor(Func<Task> action) : SaveChangesInterceptor
{
    private int fired;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref fired, 1) == 0)
        {
            await action();
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Components.Authorization;
using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Genanvendelig guard for "AdminApiPolicy" rate limiting (§4.1). Da admin-editors kalder
/// deres I*AdminService direkte via DI (intet HTTP API-lag), kan
/// Microsoft.AspNetCore.RateLimiting-middleware ikke ramme dem — denne klasse decorer'er
/// i stedet handlingen direkte, i samme stil som StepUpGuard.
///
/// Rækkefølge: kaldes INDENI StepUpGuard.RunAsync, så kvote-permit'en først forbruges, når
/// step-up er gennemført og handlingen reelt udføres (en annulleret modal koster intet).
///
/// Bevidst IKKE en Blazor-komponent eller DI-registreret service — ejes som felt af den
/// beskyttede komponent, oprettet i OnInitializedAsync.
/// </summary>
public sealed class AdminActionGuard(IAdminActionRateLimiter rateLimiter)
{
    public const string RateLimitedMessage = "Error: For mange handlinger på kort tid. Vent et øjeblik og prøv igen.";
    public const string UserUnknownMessage = "Error: Kunne ikke bestemme den aktuelle bruger.";

    /// <summary>True når det seneste RunAsync-kald blev afvist pga. rate limiting.</summary>
    public bool IsRateLimited { get; private set; }

    /// <summary>True når det seneste RunAsync-kald blev afvist, fordi bruger-id ikke kunne bestemmes.</summary>
    public bool IsUserUnknown { get; private set; }

    /// <summary>
    /// Udfører <paramref name="action"/>, hvis brugeren er kendt og kvoten ikke er opbrugt.
    /// Ellers sættes IsRateLimited/IsUserUnknown, handlingen udføres ikke, og
    /// <paramref name="onRejected"/> kaldes med den færdige fejltekst.
    /// </summary>
    public async Task RunAsync(
        Task<AuthenticationState>? authenticationStateTask,
        Func<Task> action,
        Action<string>? onRejected = null)
    {
        IsRateLimited = false;
        IsUserUnknown = false;

        var userId = await CurrentUserIdResolver.ResolveAsync(authenticationStateTask);
        if (userId is null)
        {
            IsUserUnknown = true;
            onRejected?.Invoke(UserUnknownMessage);
            return;
        }

        if (!await rateLimiter.TryAcquireAsync(userId))
        {
            IsRateLimited = true;
            onRejected?.Invoke(RateLimitedMessage);
            return;
        }

        await action();
    }
}
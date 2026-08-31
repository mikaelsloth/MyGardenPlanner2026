namespace MyGardenPlanner2026.Components.Account.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

/// <summary>
/// Genanvendelig guard for step-up re-autentificering af følsomme handlinger i admin-
/// editors (§3.2). Kapsler ShowModal/pendingAction-tilstanden og selve håndhævelsen mod
/// den angivne policy, så en komponent blot kalder RunAsync omkring den handling, der
/// skal beskyttes, og binder ShowModal/ExecutePendingActionAsync/Cancel til en
/// &lt;StepUpReAuthModal&gt;.
///
/// Bevidst IKKE en Blazor-komponent eller DI-registreret service — en almindelig klasse
/// ejet som felt af den beskyttede komponent, oprettet i OnInitializedAsync. Tilstanden
/// er dermed isoleret pr. komponent-instans, ligesom hver editor havde det duplikeret før
/// denne udtrækning.
/// </summary>
public sealed class StepUpGuard(IAuthorizationService authorizationService, string policyName)
{
    private Func<Task>? pendingAction;

    /// <summary>True når re-autentificering er nødvendig, og modalen skal vises.</summary>
    public bool ShowModal { get; private set; }

    /// <summary>
    /// Udfører <paramref name="action"/> med det samme, hvis brugeren har en gyldig,
    /// nylig re-autentificering. Ellers gemmes handlingen, og ShowModal sættes til true.
    /// </summary>
    public async Task RunAsync(Task<AuthenticationState>? authenticationStateTask, Func<Task> action)
    {
        if (await HasRecentAuthenticationAsync(authenticationStateTask))
        {
            await action();
            return;
        }

        pendingAction = action;
        ShowModal = true;
    }

    /// <summary>Kaldes fra &lt;StepUpReAuthModal OnReAuthenticated&gt; — udfører den gemte handling.</summary>
    public async Task ExecutePendingActionAsync()
    {
        ShowModal = false;

        if (pendingAction is not null)
        {
            var action = pendingAction;
            pendingAction = null;
            await action();
        }
    }

    /// <summary>Kaldes fra &lt;StepUpReAuthModal OnCancel&gt; — kasserer den gemte handling.</summary>
    public void Cancel()
    {
        ShowModal = false;
        pendingAction = null;
    }

    private async Task<bool> HasRecentAuthenticationAsync(Task<AuthenticationState>? authenticationStateTask)
    {
        if (authenticationStateTask is null)
        {
            return false;
        }

        var authState = await authenticationStateTask;
        var result = await authorizationService.AuthorizeAsync(authState.User, resource: null, policyName);

        return result.Succeeded;
    }
}
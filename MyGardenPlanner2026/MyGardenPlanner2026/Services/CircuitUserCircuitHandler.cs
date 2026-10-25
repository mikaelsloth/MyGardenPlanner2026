namespace MyGardenPlanner2026.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

/// <summary>
/// Sætter CircuitUserContext for hver indgående circuit-aktivitet (UI-events, JS-interop), så
/// AuditLoggingInterceptor kan se, hvem der udfører en handling i en Blazor Server-circuit.
/// Registreres Scoped som CircuitHandler, så AuthenticationStateProvider er circuitens egen.
/// </summary>
public sealed class CircuitUserCircuitHandler(AuthenticationStateProvider authenticationStateProvider)
    : CircuitHandler
{
    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            CircuitUserContext.Current = await ResolveUserAsync();

            try
            {
                await next(context);
            }
            finally
            {
                CircuitUserContext.Current = null;
            }
        };

    private async Task<Core.Contracts.Common.CurrentUserInfo?> ResolveUserAsync()
    {
        try
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            return CircuitUserContext.From(state.User);
        }
        catch (InvalidOperationException)
        {
            // Authentication state er ikke sat endnu (tidlig circuit-aktivitet): ingen bruger.
            return null;
        }
    }
}
namespace MyGardenPlanner2026.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Sætter CircuitUserContext for hver indgående circuit-aktivitet (UI-events, JS-interop), så
/// AuditLoggingInterceptor kan se, hvem der udfører en handling i en Blazor Server-circuit.
/// Registreres Scoped som CircuitHandler, så AuthenticationStateProvider er circuitens egen.
/// Klientens IP-adresse læses fra IHttpContextAccessor, når HttpContext er tilgængelig
/// (circuit åbnet, forbindelse genoprettet, aktivitet) og huskes for circuitens levetid,
/// da HttpContext normalt er null i almindelige event-handlere.
/// </summary>
public sealed class CircuitUserCircuitHandler(
    AuthenticationStateProvider authenticationStateProvider,
    IHttpContextAccessor httpContextAccessor)
    : CircuitHandler
{
    private string? clientIpAddress;

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CaptureClientIp();
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        CaptureClientIp();
        return Task.CompletedTask;
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next) =>
        async context =>
        {
            CaptureClientIp();
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

    private void CaptureClientIp()
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        if (ipAddress is not null)
        {
            clientIpAddress = ipAddress;
        }
    }

    private async Task<Core.Contracts.Common.CurrentUserInfo?> ResolveUserAsync()
    {
        try
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            return CircuitUserContext.From(state.User, clientIpAddress);
        }
        catch (InvalidOperationException)
        {
            // Authentication state er ikke sat endnu (tidlig circuit-aktivitet): ingen bruger.
            return null;
        }
    }
}
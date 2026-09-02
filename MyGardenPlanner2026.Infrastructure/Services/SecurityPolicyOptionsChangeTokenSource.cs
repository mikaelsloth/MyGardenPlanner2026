namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Bruges af IOptionsMonitor&lt;TOptions&gt; til at vide, hvornår CurrentValue skal
/// genberegnes. Én instans pr. options-type, alle bakket af den samme delte
/// SecurityPolicyChangeSignal-singleton.
/// </summary>
public sealed class SecurityPolicyOptionsChangeTokenSource<TOptions>(SecurityPolicyChangeSignal signal)
    : IOptionsChangeTokenSource<TOptions>
    where TOptions : class
{
    public string? Name => Options.DefaultName;

    public IChangeToken GetChangeToken() => signal.GetOrCreateSource<TOptions>().GetChangeToken();
}
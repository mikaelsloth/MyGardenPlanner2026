namespace MyGardenPlanner2026.Infrastructure.Services;

using Microsoft.Extensions.Primitives;
using MyGardenPlanner2026.Core.Contracts.Admin;
using System.Collections.Concurrent;

/// <summary>
/// Delt singleton der udsteder og udløser CancellationChangeToken pr. options-type.
/// ISecurityPolicyChangeSignal (kaldt af admin-services efter save) og
/// SecurityPolicyOptionsChangeTokenSource&lt;T&gt; (læst af IOptionsMonitor&lt;T&gt;)
/// deler samme instans via DI, så et TriggerChange&lt;T&gt;-kald øjeblikkeligt får
/// IOptionsMonitor&lt;T&gt;.CurrentValue til at genberegnes ved næste tilgang.
/// </summary>
public sealed class SecurityPolicyChangeSignal : ISecurityPolicyChangeSignal
{
    private readonly ConcurrentDictionary<Type, TokenSource> _sources = new();

    public void TriggerChange<TOptions>() where TOptions : class => GetOrCreateSource<TOptions>().Trigger();

    internal TokenSource GetOrCreateSource<TOptions>() where TOptions : class =>
        _sources.GetOrAdd(typeof(TOptions), _ => new TokenSource());

    internal sealed class TokenSource
    {
        private CancellationTokenSource _cts = new();

        public IChangeToken GetChangeToken() => new CancellationChangeToken(_cts.Token);

        public void Trigger()
        {
            var previous = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
            previous.Cancel();
            previous.Dispose();
        }
    }
}
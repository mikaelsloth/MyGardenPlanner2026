namespace MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Singleton signal-hub der udløser genindlæsning af runtime-konfigurerbare
/// sikkerhedspolicies (IOptionsMonitor&lt;T&gt;.CurrentValue) uden proces-genstart.
/// Implementeres i Infrastructure via IOptionsChangeTokenSource&lt;T&gt; (samme delte
/// instans). Kaldes af admin-services EFTER et succesfuldt save til databasen.
/// </summary>
public interface ISecurityPolicyChangeSignal
{
    void TriggerChange<TOptions>() where TOptions : class;
}
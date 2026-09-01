namespace MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Markerer en entity som en runtime-konfigurerbar sikkerhedspolicy-indstilling, hvoraf
/// der kun nogensinde findes ÉN række (identificeret ved en fast, kendt Guid pr. type).
/// Bruges af PlannerDbContext til generisk admin-schema/temporal-konfiguration og af
/// SecurityPolicySettingsSeeder&lt;T&gt; til generisk idempotent seeding.
/// </summary>
public interface ISingletonSettings
{
    Guid Id { get; }
}
namespace MyGardenPlanner2026.Infrastructure.Services;

/// <summary>
/// Konfigurerbar grænse for hvor gammel en step-up re-autentificering må være,
/// bundet fra appsettings under sektionen "ReAuthenticationPolicy".
/// </summary>
public sealed class ReAuthenticationPolicyOptions
{
    public const string SectionName = "ReAuthenticationPolicy";

    public int MaxAgeMinutes { get; set; } = 15;
}
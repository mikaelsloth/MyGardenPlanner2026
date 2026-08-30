namespace MyGardenPlanner2026.Infrastructure.Services;

using MyGardenPlanner2026.Core.Contracts.Admin;

/// <summary>
/// Standardimplementering af IReAuthenticationService. Registreres Scoped, så tilstanden
/// er isoleret pr. Blazor Server-circuit ("session" jf. specifikationen) og automatisk
/// nulstilles ved fuld gen-forbindelse — tilsigtet for step-up-autentificering.
/// TimeProvider injiceres for testbarhed (samme mønster som JitElevationService).
/// </summary>
public sealed class ReAuthenticationService(TimeProvider timeProvider) : IReAuthenticationService
{
    public DateTimeOffset? LastAuthTimestampUtc { get; private set; }

    public void MarkReAuthenticated() => LastAuthTimestampUtc = timeProvider.GetUtcNow();

    public bool IsReAuthValid(TimeSpan maxAge) =>
        LastAuthTimestampUtc is { } timestamp && timeProvider.GetUtcNow() - timestamp <= maxAge;
}
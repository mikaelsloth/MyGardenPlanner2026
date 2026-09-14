namespace MyGardenPlanner2026.Core.Entities.Gardens;

/// <summary>
/// Placeholder-entitet for en haves kontaktperson. Udbygges senere, når Layer 1-stamdata
/// for kontakter designes (jf. Architecture.md). Indeholder bevidst kun et Id.
/// </summary>
public class Contact
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
namespace MyGardenPlanner2026.Core.Entities.Gardens;

/// <summary>
/// Placeholder-entitet for en haves adresse. Udbygges senere, når Layer 1-stamdata for
/// adresser designes (jf. Architecture.md). Indeholder bevidst kun et Id, så
/// Garden.AddressId kan referere til en fremtidig fuld model uden migration af Garden-tabellen.
/// </summary>
public class Address
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
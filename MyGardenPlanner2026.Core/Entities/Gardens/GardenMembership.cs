namespace MyGardenPlanner2026.Core.Entities.Gardens;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Forbinder en bruger til en have med en given rolle (Layer/Category). Svarer til
/// "HaveMedlem" i domænesproget. Præcis én GardenMembership pr. (GardenId, UserId) må
/// have IsOwner == true — håndhæves i IOnboardingService, ikke i datalaget.
/// </summary>
public class GardenMembership : ISoftDelete
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public Guid GardenId { get; set; }
    public Garden? Garden { get; set; }

    public string UserId { get; set; } = string.Empty;

    /// <summary>True for den primære opretter/ejer af haven.</summary>
    public bool IsOwner { get; set; }

    public GardenAccessLevel Layer { get; set; }
    public AccessCategory Category { get; set; }

    public DateTimeOffset JoinedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
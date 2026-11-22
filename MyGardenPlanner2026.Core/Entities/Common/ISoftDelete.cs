namespace MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Markerer en entity som beskyttet: soft-delete anvendes automatisk via
/// SoftDeleteInterceptor (se Infrastructure/Interceptors).
/// DeletedByUserId = den handlende brugers ID (pseudonymt, bevares), eller en systemlabel
/// "system-&lt;klasse&gt;.&lt;metode&gt;" (se SystemActorContext).
/// DeletedByEmail = den handlende brugers e-mail; nulstilles, når brugeren slettes.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
    string? DeletedByUserId { get; set; }
    string? DeletedByEmail { get; set; }
}
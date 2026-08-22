namespace MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Markerer en entity som beskyttet: soft-delete anvendes automatisk via
/// SoftDeleteInterceptor (se Infrastructure/Interceptors).
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAtUtc { get; set; }
    string? DeletedBy { get; set; }
}
namespace MyGardenPlanner2026.Core.Entities.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Trappe-rabat pr. antal haver. Baseret på Prismatrix.md, tabel 2 ("Trappe-sats %").
/// MaxGardens = null for sidste (åbne) trappe.
/// </summary>
public class GardenVolumeDiscountTier : ISoftDelete
{
    public int Id { get; set; }

    public int MinGardens { get; set; }
    public int? MaxGardens { get; set; }

    public decimal PriceMultiplier { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
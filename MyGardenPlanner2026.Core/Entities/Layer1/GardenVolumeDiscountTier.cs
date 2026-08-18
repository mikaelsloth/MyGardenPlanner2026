namespace MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Trappe-rabat pr. antal haver. Baseret på Prismatrix.md, tabel 2 ("Trappe-sats %").
/// MaxGardens = null for sidste (åbne) trappe.
/// </summary>
public class GardenVolumeDiscountTier
{
    public int Id { get; set; }

    public int MinGardens { get; set; }
    public int? MaxGardens { get; set; }

    /// <summary>Andel af basispris pr. have, fx 1.00m, 0.90m, 0.40m.</summary>
    public decimal PriceMultiplier { get; set; }

    public int DisplayOrder { get; set; }
}
namespace MyGardenPlanner2026.Core.Entities.Layer1;

using MyGardenPlanner2026.Core.Entities.Common;

/// <summary>
/// Tilkøbsmodul jf. Prismatrix.md, tabel 3.
/// </summary>
public class SubscriptionAddOn : ISoftDelete
{
    public int Id { get; set; }

    public AddOnType Type { get; set; }

    public string Name { get; set; } = string.Empty;
    public string UnitDescription { get; set; } = string.Empty;

    public decimal AnnualPrice { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal PerpetualPrice { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
}
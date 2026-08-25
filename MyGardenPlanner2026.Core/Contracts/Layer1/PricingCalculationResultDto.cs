namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public sealed record PricingCalculationResultDto(
    decimal BasePricePerGarden,
    decimal WeightedGardenCount,
    decimal DiscountMultiplier,
    decimal GardenSubtotal,
    IReadOnlyList<AddOnLineItemDto> AddOnLineItems,
    decimal AddOnsTotal,
    decimal Total);

public sealed record AddOnLineItemDto(
    Guid AddOnId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
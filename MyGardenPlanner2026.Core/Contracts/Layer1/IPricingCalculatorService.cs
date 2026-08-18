namespace MyGardenPlanner2026.Core.Contracts.Layer1;

public interface IPricingCalculatorService
{
    Task<PricingCalculationResultDto> CalculateAsync(
        PricingCalculationRequestDto request,
        CancellationToken cancellationToken = default);
}
namespace MyGardenPlanner2026.Tests.Unit.Services;

using FluentAssertions;
using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;
using MyGardenPlanner2026.Infrastructure.Data.Seed;
using MyGardenPlanner2026.Infrastructure.Services;
using Xunit;

public class PricingCalculatorServiceTests : TestDbContext
{
    private async Task SeedAllAsync()
    {
        var tierSeeder = new SubscriptionTierSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionTierCatalog());
        await tierSeeder.SeedAsync(TestContext.Current.CancellationToken);

        var volumeSeeder = new GardenVolumeDiscountSeeder(CreateAdminDbContextFactory(), new DefaultGardenVolumeDiscountCatalog());
        await volumeSeeder.SeedAsync(TestContext.Current.CancellationToken);

        var addOnSeeder = new SubscriptionAddOnSeeder(CreateAdminDbContextFactory(), new DefaultSubscriptionAddOnCatalog());
        await addOnSeeder.SeedAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CalculateAsync_OneGardenAdministratorLag1_ReturnsExactPrismatrixPrice()
    {
        await SeedAllAsync();
        var service = new PricingCalculatorService(CreateDbContextFactory());

        var request = new PricingCalculationRequestDto(
            GardenAccessLevel.HaveArkitekt,
            AccessCategory.Administrator,
            BillingCycle.Annual,
            ActiveGardens: 1,
            ArchivedGardens: 0,
            AddOnQuantities: new Dictionary<Guid, int>());

        var result = await service.CalculateAsync(request, TestContext.Current.CancellationToken);

        result.BasePricePerGarden.Should().Be(336m);
        result.WeightedGardenCount.Should().Be(1m);
        result.DiscountMultiplier.Should().Be(1.00m);
        result.GardenSubtotal.Should().Be(336m);
        result.Total.Should().Be(336m);
    }

    [Fact]
    public async Task CalculateAsync_SixActiveGardensEditor_Uses80PercentTier()
    {
        await SeedAllAsync();
        var service = new PricingCalculatorService(CreateDbContextFactory());

        var request = new PricingCalculationRequestDto(
            GardenAccessLevel.BedDesigner,
            AccessCategory.Editor,
            BillingCycle.Annual,
            ActiveGardens: 6,
            ArchivedGardens: 0,
            AddOnQuantities: new Dictionary<Guid, int>());

        var result = await service.CalculateAsync(request, TestContext.Current.CancellationToken);

        result.WeightedGardenCount.Should().Be(6m);
        result.DiscountMultiplier.Should().Be(0.80m);
        result.GardenSubtotal.Should().Be(120m * 0.80m * 6m);
    }

    [Fact]
    public async Task CalculateAsync_ArchivedGardens_WeightedDifferentlyForAdministratorVsOtherCategories()
    {
        await SeedAllAsync();
        var service = new PricingCalculatorService(CreateDbContextFactory());

        var adminRequest = new PricingCalculationRequestDto(
            GardenAccessLevel.Planlaegger,
            AccessCategory.Administrator,
            BillingCycle.Annual,
            ActiveGardens: 0,
            ArchivedGardens: 4,
            AddOnQuantities: new Dictionary<Guid, int>());

        var editorRequest = adminRequest with { AccessCategory = AccessCategory.Editor };

        var adminResult = await service.CalculateAsync(adminRequest, TestContext.Current.CancellationToken);
        var editorResult = await service.CalculateAsync(editorRequest, TestContext.Current.CancellationToken);

        adminResult.WeightedGardenCount.Should().Be(1.0m);   // 4 * 0.25
        adminResult.DiscountMultiplier.Should().Be(1.00m);   // "1 have"-trappe

        editorResult.WeightedGardenCount.Should().Be(4.0m);  // 4 * 1.0
        editorResult.DiscountMultiplier.Should().Be(0.90m);  // "2-5 haver"-trappe
    }

    [Fact]
    public async Task CalculateAsync_WithAddOns_IncludesAddOnsInTotal()
    {
        await SeedAllAsync();

        using var seededContext = CreateDbContext();
        var bedforslagAddOn = seededContext.SubscriptionAddOns.Single(a => a.Type == AddOnType.BedforslagNiveau2);

        var service = new PricingCalculatorService(CreateDbContextFactory());

        var request = new PricingCalculationRequestDto(
            GardenAccessLevel.HaveArkitekt,
            AccessCategory.Administrator,
            BillingCycle.Annual,
            ActiveGardens: 1,
            ArchivedGardens: 0,
            AddOnQuantities: new Dictionary<Guid, int> { [bedforslagAddOn.Id] = 2 });

        var result = await service.CalculateAsync(request, TestContext.Current.CancellationToken);

        result.AddOnsTotal.Should().Be(180m * 2);
        result.Total.Should().Be(336m + (180m * 2));
    }

    [Fact]
    public async Task CalculateAsync_PerpetualBillingCycle_UsesPerpetualBasePriceAndAddOnPrices()
    {
        await SeedAllAsync();

        using var seededContext = CreateDbContext();
        var bedforslagAddOn = seededContext.SubscriptionAddOns.Single(a => a.Type == AddOnType.BedforslagNiveau2);

        var service = new PricingCalculatorService(CreateDbContextFactory());

        var request = new PricingCalculationRequestDto(
            GardenAccessLevel.HaveArkitekt,
            AccessCategory.Administrator,
            BillingCycle.Perpetual,
            ActiveGardens: 1,
            ArchivedGardens: 0,
            AddOnQuantities: new Dictionary<Guid, int> { [bedforslagAddOn.Id] = 1 });

        var result = await service.CalculateAsync(request, TestContext.Current.CancellationToken);

        result.BasePricePerGarden.Should().Be(840m); // SubscriptionTier.PerpetualPrice for Lag1/Admin
        result.GardenSubtotal.Should().Be(840m);
        result.AddOnsTotal.Should().Be(450m); // Bedforslag Perpetual-pris
        result.Total.Should().Be(840m + 450m);
    }

    [Fact]
    public async Task CalculateAsync_PerpetualBillingCycle_AppliesSameVolumeDiscountTrapAsAnnual()
    {
        await SeedAllAsync();
        var service = new PricingCalculatorService(CreateDbContextFactory());

        var request = new PricingCalculationRequestDto(
            GardenAccessLevel.BedDesigner,
            AccessCategory.Editor,
            BillingCycle.Perpetual,
            ActiveGardens: 6,
            ArchivedGardens: 0,
            AddOnQuantities: new Dictionary<Guid, int>());

        var result = await service.CalculateAsync(request, TestContext.Current.CancellationToken);

        result.DiscountMultiplier.Should().Be(0.80m); // Samme trappe som Annual-testen
        result.BasePricePerGarden.Should().Be(300m);  // SubscriptionTier.PerpetualPrice for Lag2/Editor
    }
}
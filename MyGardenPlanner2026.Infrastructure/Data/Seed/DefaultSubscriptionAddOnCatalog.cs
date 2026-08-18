namespace MyGardenPlanner2026.Infrastructure.Data.Seed;

using MyGardenPlanner2026.Core.Contracts.Layer1;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Core.Entities.Layer1;

/// <summary>
/// Tilkøbsmoduler fra Prismatrix.md, tabel 3.
/// </summary>
public sealed class DefaultSubscriptionAddOnCatalog : ISubscriptionAddOnCatalog
{
    public IReadOnlyList<SubscriptionAddOn> GetDefaultAddOns() =>
    [
        new()
        {
            Type = AddOnType.BedforslagNiveau2,
            Name = "Bedforslag (Niveau 2)",
            UnitDescription = "Pakke med 2 bedforslag",
            AnnualPrice = 180m,
            MonthlyPrice = 15m,
            DisplayOrder = 1
        },
        new()
        {
            Type = AddOnType.BedeINiveau2,
            Name = "Bede i Bedforslag (Niveau 2)",
            UnitDescription = "Pakke med 25 bede",
            AnnualPrice = 90m,
            MonthlyPrice = 7.5m,
            DisplayOrder = 2
        },
        new()
        {
            Type = AddOnType.PlanlagteBedeNiveau3,
            Name = "Planlagte bede (Niveau 3)",
            UnitDescription = "Pakke med 25 bede",
            AnnualPrice = 144m,
            MonthlyPrice = 12m,
            DisplayOrder = 3
        },
        new()
        {
            Type = AddOnType.ArtefaktpakkeA,
            Name = "Artefaktpakke A",
            UnitDescription = "+25 Planter / Materialer / Opgavelister",
            AnnualPrice = 48m,
            MonthlyPrice = 4m,
            DisplayOrder = 4
        },
        new()
        {
            Type = AddOnType.ArtefaktpakkeB,
            Name = "Artefaktpakke B",
            UnitDescription = "+5 Konstruktioner / Lejeaftaler / Tilbud",
            AnnualPrice = 24m,
            MonthlyPrice = 2m,
            DisplayOrder = 5
        }
    ];
}
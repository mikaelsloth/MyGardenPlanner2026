namespace MyGardenPlanner2026.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Separat factory-type til DbContext-instanser der forbinder som den privilegerede
/// admin-databasebruger (adgang til admin-schema). Bruges KUN af admin-services og
/// seedere for de tre beskyttede entities. Standard IDbContextFactory&lt;PlannerDbContext&gt;
/// forbinder som den begrænsede app-bruger (kun SELECT på admin-schema).
/// </summary>
public interface IAdminDbContextFactory : IDbContextFactory<PlannerDbContext>
{
}
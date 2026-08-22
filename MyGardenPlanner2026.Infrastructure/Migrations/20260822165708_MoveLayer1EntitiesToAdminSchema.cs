using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveLayer1EntitiesToAdminSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.RenameTable(
                name: "SubscriptionTiers",
                newName: "SubscriptionTiers",
                newSchema: "admin");

            migrationBuilder.RenameTable(
                name: "SubscriptionAddOns",
                newName: "SubscriptionAddOns",
                newSchema: "admin");

            migrationBuilder.RenameTable(
                name: "GardenVolumeDiscountTiers",
                newName: "GardenVolumeDiscountTiers",
                newSchema: "admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SubscriptionTiers",
                schema: "admin",
                newName: "SubscriptionTiers");

            migrationBuilder.RenameTable(
                name: "SubscriptionAddOns",
                schema: "admin",
                newName: "SubscriptionAddOns");

            migrationBuilder.RenameTable(
                name: "GardenVolumeDiscountTiers",
                schema: "admin",
                newName: "GardenVolumeDiscountTiers");
        }
    }
}

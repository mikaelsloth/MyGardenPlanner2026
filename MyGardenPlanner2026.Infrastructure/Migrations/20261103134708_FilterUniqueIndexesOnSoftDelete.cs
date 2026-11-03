using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterUniqueIndexesOnSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAddOns_Type",
                schema: "admin",
                table: "SubscriptionAddOns");

            migrationBuilder.DropIndex(
                name: "IX_GardenVolumeDiscountTiers_MinGardens",
                schema: "admin",
                table: "GardenVolumeDiscountTiers");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAddOns_Type",
                schema: "admin",
                table: "SubscriptionAddOns",
                column: "Type",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_GardenVolumeDiscountTiers_MinGardens",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                column: "MinGardens",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubscriptionAddOns_Type",
                schema: "admin",
                table: "SubscriptionAddOns");

            migrationBuilder.DropIndex(
                name: "IX_GardenVolumeDiscountTiers_MinGardens",
                schema: "admin",
                table: "GardenVolumeDiscountTiers");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAddOns_Type",
                schema: "admin",
                table: "SubscriptionAddOns",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GardenVolumeDiscountTiers_MinGardens",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                column: "MinGardens",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGardenVolumeDiscountAndAddOns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GardenVolumeDiscountTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinGardens = table.Column<int>(type: "int", nullable: false),
                    MaxGardens = table.Column<int>(type: "int", nullable: true),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GardenVolumeDiscountTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionAddOns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnnualPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionAddOns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GardenVolumeDiscountTiers_MinGardens",
                table: "GardenVolumeDiscountTiers",
                column: "MinGardens",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionAddOns_Type",
                table: "SubscriptionAddOns",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GardenVolumeDiscountTiers");

            migrationBuilder.DropTable(
                name: "SubscriptionAddOns");
        }
    }
}

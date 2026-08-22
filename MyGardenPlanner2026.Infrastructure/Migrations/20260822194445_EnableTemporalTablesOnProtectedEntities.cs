using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnableTemporalTablesOnProtectedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "SubscriptionTiers",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SubscriptionTiersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.AlterTable(
                name: "SubscriptionAddOns",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "SubscriptionAddOnsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.AlterTable(
                name: "GardenVolumeDiscountTiers",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "GardenVolumeDiscountTiersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFromUtc",
                schema: "admin",
                table: "SubscriptionTiers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:TemporalIsPeriodStartColumn", true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidToUtc",
                schema: "admin",
                table: "SubscriptionTiers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:TemporalIsPeriodEndColumn", true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFromUtc",
                schema: "admin",
                table: "SubscriptionAddOns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:TemporalIsPeriodStartColumn", true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidToUtc",
                schema: "admin",
                table: "SubscriptionAddOns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:TemporalIsPeriodEndColumn", true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFromUtc",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:TemporalIsPeriodStartColumn", true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidToUtc",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("SqlServer:TemporalIsPeriodEndColumn", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValidFromUtc",
                schema: "admin",
                table: "SubscriptionTiers")
                .Annotation("SqlServer:TemporalIsPeriodStartColumn", true);

            migrationBuilder.DropColumn(
                name: "ValidToUtc",
                schema: "admin",
                table: "SubscriptionTiers")
                .Annotation("SqlServer:TemporalIsPeriodEndColumn", true);

            migrationBuilder.DropColumn(
                name: "ValidFromUtc",
                schema: "admin",
                table: "SubscriptionAddOns")
                .Annotation("SqlServer:TemporalIsPeriodStartColumn", true);

            migrationBuilder.DropColumn(
                name: "ValidToUtc",
                schema: "admin",
                table: "SubscriptionAddOns")
                .Annotation("SqlServer:TemporalIsPeriodEndColumn", true);

            migrationBuilder.DropColumn(
                name: "ValidFromUtc",
                schema: "admin",
                table: "GardenVolumeDiscountTiers")
                .Annotation("SqlServer:TemporalIsPeriodStartColumn", true);

            migrationBuilder.DropColumn(
                name: "ValidToUtc",
                schema: "admin",
                table: "GardenVolumeDiscountTiers")
                .Annotation("SqlServer:TemporalIsPeriodEndColumn", true);

            migrationBuilder.AlterTable(
                name: "SubscriptionTiers",
                schema: "admin")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "SubscriptionTiersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.AlterTable(
                name: "SubscriptionAddOns",
                schema: "admin")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "SubscriptionAddOnsHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.AlterTable(
                name: "GardenVolumeDiscountTiers",
                schema: "admin")
                .OldAnnotation("SqlServer:IsTemporal", true)
                .OldAnnotation("SqlServer:TemporalHistoryTableName", "GardenVolumeDiscountTiersHistory")
                .OldAnnotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .OldAnnotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .OldAnnotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");
        }
    }
}

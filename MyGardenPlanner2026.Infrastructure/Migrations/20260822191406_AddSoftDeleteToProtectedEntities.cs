using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToProtectedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "admin",
                table: "SubscriptionTiers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "admin",
                table: "SubscriptionTiers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "admin",
                table: "SubscriptionTiers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "admin",
                table: "SubscriptionAddOns",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "admin",
                table: "SubscriptionAddOns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "admin",
                table: "SubscriptionAddOns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "admin",
                table: "GardenVolumeDiscountTiers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "admin",
                table: "SubscriptionTiers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "admin",
                table: "SubscriptionTiers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "admin",
                table: "SubscriptionTiers");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "admin",
                table: "SubscriptionAddOns");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "admin",
                table: "SubscriptionAddOns");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "admin",
                table: "SubscriptionAddOns");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "admin",
                table: "GardenVolumeDiscountTiers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "admin",
                table: "GardenVolumeDiscountTiers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "admin",
                table: "GardenVolumeDiscountTiers");
        }
    }
}

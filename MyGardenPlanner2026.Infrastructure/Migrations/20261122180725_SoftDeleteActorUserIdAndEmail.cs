using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261122180725_SoftDeleteActorUserIdAndEmail : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DeletedBy",
            table: "UserEntitlements");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "SubscriptionTiers");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "SubscriptionAddOns");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "ReAuthFailureTrackerSettings");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "ReAuthenticationPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "LoginRateLimitSettings");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "JitElevationPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "GardenVolumeDiscountTiers");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            table: "Gardens");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            table: "GardenMemberships");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            table: "GardenInvitations");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "AuditLogExportJobPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "AdminApiRateLimitSettings");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            schema: "admin",
            table: "RoleElevationRequests");

        migrationBuilder.AddColumn<string>(
           name: "DeletedByUserId",
           table: "RoleElevationRequests",
           type: "nvarchar(450)",
           maxLength: 450,
           nullable: true);


        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            table: "UserEntitlements",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            table: "UserEntitlements",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "SubscriptionTiers",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "SubscriptionTiers",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "SubscriptionAddOns",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "SubscriptionAddOns",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "RoleElevationRequests",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "ReAuthFailureTrackerSettings",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "ReAuthFailureTrackerSettings",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "ReAuthenticationPolicySettings",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "ReAuthenticationPolicySettings",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "LoginRateLimitSettings",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "LoginRateLimitSettings",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "JitElevationPolicySettings",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "JitElevationPolicySettings",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "GardenVolumeDiscountTiers",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "GardenVolumeDiscountTiers",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            table: "Gardens",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            table: "Gardens",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            table: "GardenMemberships",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            table: "GardenMemberships",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            table: "GardenInvitations",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            table: "GardenInvitations",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "AuditLogExportJobPolicySettings",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "AuditLogExportJobPolicySettings",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByEmail",
            schema: "admin",
            table: "AdminApiRateLimitSettings",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedByUserId",
            schema: "admin",
            table: "AdminApiRateLimitSettings",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            table: "UserEntitlements");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            table: "UserEntitlements");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "SubscriptionTiers");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "SubscriptionTiers");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "SubscriptionAddOns");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "SubscriptionAddOns");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "RoleElevationRequests");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "ReAuthFailureTrackerSettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "ReAuthFailureTrackerSettings");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "ReAuthenticationPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "ReAuthenticationPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "LoginRateLimitSettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "LoginRateLimitSettings");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "JitElevationPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "JitElevationPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "GardenVolumeDiscountTiers");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "GardenVolumeDiscountTiers");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            table: "Gardens");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            table: "Gardens");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            table: "GardenMemberships");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            table: "GardenMemberships");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            table: "GardenInvitations");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            table: "GardenInvitations");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "AuditLogExportJobPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "AuditLogExportJobPolicySettings");

        migrationBuilder.DropColumn(
            name: "DeletedByEmail",
            schema: "admin",
            table: "AdminApiRateLimitSettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "AdminApiRateLimitSettings");

        migrationBuilder.DropColumn(
            name: "DeletedByUserId",
            schema: "admin",
            table: "RoleElevationRequests");

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            table: "RoleElevationRequests",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            table: "UserEntitlements",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "SubscriptionTiers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "SubscriptionAddOns",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "ReAuthFailureTrackerSettings",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "ReAuthenticationPolicySettings",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "LoginRateLimitSettings",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "JitElevationPolicySettings",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "GardenVolumeDiscountTiers",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            table: "Gardens",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            table: "GardenMemberships",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            table: "GardenInvitations",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "AuditLogExportJobPolicySettings",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DeletedBy",
            schema: "admin",
            table: "AdminApiRateLimitSettings",
            type: "nvarchar(max)",
            nullable: true);
    }
}

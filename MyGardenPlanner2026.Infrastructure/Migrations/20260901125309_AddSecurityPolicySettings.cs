using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityPolicySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminApiRateLimitSettings",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermitLimit = table.Column<int>(type: "int", nullable: false),
                    WindowSeconds = table.Column<int>(type: "int", nullable: false),
                    SegmentsPerWindow = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminApiRateLimitSettings", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AdminApiRateLimitSettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.CreateTable(
                name: "JitElevationPolicySettings",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinRequestedMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxRequestedMinutes = table.Column<int>(type: "int", nullable: false),
                    SweepIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JitElevationPolicySettings", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "JitElevationPolicySettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.CreateTable(
                name: "LoginRateLimitSettings",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermitLimit = table.Column<int>(type: "int", nullable: false),
                    WindowSeconds = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginRateLimitSettings", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LoginRateLimitSettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.CreateTable(
                name: "ReAuthenticationPolicySettings",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxAgeMinutes = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReAuthenticationPolicySettings", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ReAuthenticationPolicySettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.CreateTable(
                name: "ReAuthFailureTrackerSettings",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Threshold = table.Column<int>(type: "int", nullable: false),
                    WindowDays = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReAuthFailureTrackerSettings", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ReAuthFailureTrackerSettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminApiRateLimitSettings",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AdminApiRateLimitSettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.DropTable(
                name: "JitElevationPolicySettings",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "JitElevationPolicySettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.DropTable(
                name: "LoginRateLimitSettings",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "LoginRateLimitSettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.DropTable(
                name: "ReAuthenticationPolicySettings",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ReAuthenticationPolicySettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");

            migrationBuilder.DropTable(
                name: "ReAuthFailureTrackerSettings",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ReAuthFailureTrackerSettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");
        }
    }
}

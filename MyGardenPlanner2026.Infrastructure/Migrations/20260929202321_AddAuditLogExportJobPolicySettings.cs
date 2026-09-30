using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogExportJobPolicySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogExportJobPolicySettings",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetentionHours = table.Column<int>(type: "int", nullable: false),
                    MaxActiveJobsPerUser = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AuditLogExportJobPolicySettings", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AuditLogExportJobPolicySettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogExportJobPolicySettings",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "AuditLogExportJobPolicySettingsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "ValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "ValidFromUtc");
        }
    }
}

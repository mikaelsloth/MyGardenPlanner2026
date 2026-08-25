using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleElevationRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleElevationRequests",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequesterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ApproverUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RequestedHours = table.Column<int>(type: "int", nullable: false),
                    ValidFromUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValidToUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SysValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    SysValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleElevationRequests", x => x.Id);
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "RoleElevationRequestsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysValidFromUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RoleElevationRequests_RequesterUserId_Status",
                schema: "admin",
                table: "RoleElevationRequests",
                columns: new[] { "RequesterUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RoleElevationRequests_RoleName_Status",
                schema: "admin",
                table: "RoleElevationRequests",
                columns: new[] { "RoleName", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleElevationRequests",
                schema: "admin")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "RoleElevationRequestsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", "admin")
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "SysValidToUtc")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "SysValidFromUtc");
        }
    }
}

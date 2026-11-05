using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddXlsxMaxRowsToAuditLogExportJobPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "XlsxMaxRows",
                schema: "admin",
                table: "AuditLogExportJobPolicySettings",
                type: "int",
                nullable: false,
                defaultValue: 50000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XlsxMaxRows",
                schema: "admin",
                table: "AuditLogExportJobPolicySettings");
        }
    }
}

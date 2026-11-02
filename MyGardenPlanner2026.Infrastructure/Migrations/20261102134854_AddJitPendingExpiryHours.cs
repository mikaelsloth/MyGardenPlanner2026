using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJitPendingExpiryHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PendingExpiryHours",
                schema: "admin",
                table: "JitElevationPolicySettings",
                type: "int",
                nullable: false,
                defaultValue: 48);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingExpiryHours",
                schema: "admin",
                table: "JitElevationPolicySettings");
        }
    }
}

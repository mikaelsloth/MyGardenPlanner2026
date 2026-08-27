using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameRoleElevationRequestHoursToMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedHours",
                schema: "admin",
                table: "RoleElevationRequests",
                newName: "RequestedMinutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedMinutes",
                schema: "admin",
                table: "RoleElevationRequests",
                newName: "RequestedHours");
        }
    }
}

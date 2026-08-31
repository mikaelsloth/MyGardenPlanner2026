using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReAuthFailureAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReAuthFailureAttempts",
                schema: "admin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReAuthFailureAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReAuthFailureAttempts_UserId_OccurredAtUtc",
                schema: "admin",
                table: "ReAuthFailureAttempts",
                columns: new[] { "UserId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReAuthFailureAttempts",
                schema: "admin");
        }
    }
}

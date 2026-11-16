using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyGardenPlanner2026.Infrastructure.Migrations;

/// <inheritdoc />
public partial class _20261116220502_AddPendingGardenRestore : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PendingGardenRestores",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                GardenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnerEmailHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                FormerUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                DeletedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                ExpiresUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PendingGardenRestores", x => x.Id);
                table.ForeignKey(
                    name: "FK_PendingGardenRestores_Gardens_GardenId",
                    column: x => x.GardenId,
                    principalTable: "Gardens",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PendingGardenRestores_ExpiresUtc",
            table: "PendingGardenRestores",
            column: "ExpiresUtc");

        migrationBuilder.CreateIndex(
            name: "IX_PendingGardenRestores_GardenId",
            table: "PendingGardenRestores",
            column: "GardenId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PendingGardenRestores_OwnerEmailHash",
            table: "PendingGardenRestores",
            column: "OwnerEmailHash");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PendingGardenRestores");
    }
}

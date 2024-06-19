using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class SyncStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SyncTSUTC = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncStatuses_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncStatuses_Project_ID",
                table: "SyncStatuses",
                column: "Project_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncStatuses");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalUserStatusChangeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create PortalUserStatusChanges table
            migrationBuilder.CreateTable(
                name: "PortalUserStatusChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortalUserId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUserStatusChanges", x => x.Id);
                });

            // Add new record to Project_Roles table 
            migrationBuilder.InsertData(
                table: "Project_Roles",
                columns: new[] { "Id", "Name", "Description" },
                values: new object[] { 6, "Disabled User", "A user whose access has been disabled and cannot interact with the workspace" });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop PortalUserStatusChanges table
            migrationBuilder.DropTable(
                name: "PortalUserStatusChanges");

            // Remove the added record from Project_Roles table
            migrationBuilder.DeleteData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}

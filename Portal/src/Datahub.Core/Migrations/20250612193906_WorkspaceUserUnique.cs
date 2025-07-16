using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceUserUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Project_Users_Project_ID",
                table: "Project_Users");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Project_ID_PortalUserId",
                table: "Project_Users",
                columns: new[] { "Project_ID", "PortalUserId" },
                unique: true,
                filter: "[PortalUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Project_Users_Project_ID_PortalUserId",
                table: "Project_Users");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Project_ID",
                table: "Project_Users",
                column: "Project_ID");
        }
    }
}

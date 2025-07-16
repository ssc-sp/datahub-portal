using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class RenamePortalUserStatusChangesToPortalUserRoleChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename table from PortalUserStatusChanges to PortalUserRoleChanges
            migrationBuilder.RenameTable(
                name: "PortalUserStatusChanges",
                newName: "PortalUserRoleChanges");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "PortalUserRoleChanges", 
                newName: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert table name back to PortalUserStatusChanges
            migrationBuilder.RenameTable(
                name: "PortalUserRoleChanges",
                newName: "PortalUserStatusChanges");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "PortalUserStatusChanges",  
                newName: "StatusId");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class MetadataFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PortalUserStatusChanges",
                table: "PortalUserStatusChanges");

            migrationBuilder.RenameTable(
                name: "PortalUserStatusChanges",
                newName: "PortalUserRoleChange");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PortalUserRoleChange",
                table: "PortalUserRoleChange",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PortalUserRoleChange",
                table: "PortalUserRoleChange");

            migrationBuilder.RenameTable(
                name: "PortalUserRoleChange",
                newName: "PortalUserStatusChanges");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PortalUserStatusChanges",
                table: "PortalUserStatusChanges",
                column: "Id");
        }
    }
}

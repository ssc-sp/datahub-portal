using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class RemoveGraphGuidFromUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_PortalUsers_UserId",
                table: "UserSettings");
            
            migrationBuilder.DropColumn(
                name: "UserGuid",
                table: "UserSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserGuid",
                table: "UserSettings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }
    }
}

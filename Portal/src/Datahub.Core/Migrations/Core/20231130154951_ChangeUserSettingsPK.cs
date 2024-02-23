using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class ChangeUserSettingsPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettingsBackup");
            
            migrationBuilder.AddColumn<int>(
                name: "PortalUserId",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserSettings",
                newName: "UserGuid");

            migrationBuilder.Sql(
                "UPDATE UserSettings SET PortalUserId = (SELECT Id FROM PortalUsers WHERE PortalUsers.GraphGuid = UserSettings.UserGuid)"
            );
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings",
                column: "PortalUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_PortalUsers_PortalUserId",
                table: "UserSettings",
                column: "PortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id");

            migrationBuilder.AlterColumn<bool>(
                name: "NotificationsEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "NotificationsEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_PortalUsers_PortalUserId",
                table: "UserSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PortalUserId",
                table: "UserSettings");

            migrationBuilder.RenameColumn(
                name: "UserGuid",
                table: "UserSettings",
                newName: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSettings",
                table: "UserSettings",
                column: "UserId");
            
        }
    }
}

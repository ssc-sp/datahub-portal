using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class UserSettingsBackup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideAchievements",
                table: "PortalUsers");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "PortalUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "UserSettings",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserSettings",
                type: "int",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "HiddenAlerts",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HideAchievements",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideAlerts",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserSettingsBackup",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserId1 = table.Column<int>(type: "int", nullable: true),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    HideAchievements = table.Column<bool>(type: "bit", nullable: false),
                    HideAlerts = table.Column<bool>(type: "bit", nullable: false),
                    HiddenAlerts = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettingsBackup", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserSettingsBackup_PortalUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "PortalUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettingsBackup_UserId1",
                table: "UserSettingsBackup",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSettings_PortalUsers_UserId",
                table: "UserSettings",
                column: "UserId",
                principalTable: "PortalUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSettings_PortalUsers_UserId",
                table: "UserSettings");

            migrationBuilder.DropTable(
                name: "UserSettingsBackup");

            migrationBuilder.DropColumn(
                name: "HiddenAlerts",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "HideAchievements",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "HideAlerts",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NotificationsEnabled",
                table: "UserSettings");

            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserSettings",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<bool>(
                name: "HideAchievements",
                table: "PortalUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "PortalUsers",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);
        }
    }
}

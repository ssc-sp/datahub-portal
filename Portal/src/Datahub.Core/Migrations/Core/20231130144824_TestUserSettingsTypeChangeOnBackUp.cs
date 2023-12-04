using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class TestUserSettingsTypeChangeOnBackUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSettingsBackup",
                table: "UserSettingsBackup");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "UserSettingsBackup",
                type: "int",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSettingsBackup",
                table: "UserSettingsBackup",
                column: "UserId");
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSettingsBackup",
                table: "UserSettingsBackup");
            
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserSettingsBackup",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 64);
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSettingsBackup",
                table: "UserSettingsBackup",
                column: "UserId");
            
        }
    }
}

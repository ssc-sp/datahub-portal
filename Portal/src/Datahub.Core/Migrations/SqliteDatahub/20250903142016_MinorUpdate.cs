using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class MinorUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "UserSettings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "UserSettings");
        }
    }
}

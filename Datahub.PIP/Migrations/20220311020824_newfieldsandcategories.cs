using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class newfieldsandcategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Year",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "IndicatorAndResults");

            migrationBuilder.AddColumn<string>(
                name: "Indicator_Status",
                table: "IndicatorAndResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndicatorDetailsLocked",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIndicatorStatusLocked",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Indicator_Status",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "IsIndicatorDetailsLocked",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "IsIndicatorStatusLocked",
                table: "IndicatorAndResults");

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "Tombstones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Year",
                table: "IndicatorAndResults",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}

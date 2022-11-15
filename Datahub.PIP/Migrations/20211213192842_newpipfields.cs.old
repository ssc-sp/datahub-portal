using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class newpipfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProgramCode",
                table: "Tombstones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Tombstones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RiskCode",
                table: "Risks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IndicatorCode",
                table: "IndicatorAndResults",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "IndicatorAndResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgramCode",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "RiskCode",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "IndicatorCode",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "IndicatorAndResults");
        }
    }
}

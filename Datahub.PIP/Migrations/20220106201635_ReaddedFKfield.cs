using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class ReaddedFKfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "Tombstones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "IndicatorAndResults",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                table: "IndicatorAndResults");
        }
    }
}

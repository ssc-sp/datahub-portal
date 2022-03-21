using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class NewPipFieldsForLocking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<bool>(
                name: "IsActualResultsLocked",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatestUpdateLocked",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMethodologyLocked",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTargetLocked",
                table: "IndicatorAndResults",
                type: "bit",
                nullable: false,
                defaultValue: false);

          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndicatorAndResults_FiscalYears_FiscalYearId",
                table: "IndicatorAndResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_FiscalYears_FiscalYearId",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tombstones_FiscalYears_FiscalYearId",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "IsActualResultsLocked",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "IsLatestUpdateLocked",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "IsMethodologyLocked",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "IsTargetLocked",
                table: "IndicatorAndResults");

            migrationBuilder.AddForeignKey(
                name: "FK_IndicatorAndResults_FiscalYears_FiscalYearId",
                table: "IndicatorAndResults",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_FiscalYears_FiscalYearId",
                table: "Risks",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tombstones_FiscalYears_FiscalYearId",
                table: "Tombstones",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

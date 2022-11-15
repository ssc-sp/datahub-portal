using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class addedforeignkeyforyear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FiscalYearYearId",
                table: "Tombstones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearYearId",
                table: "Risks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearYearId",
                table: "IndicatorAndResults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tombstones_FiscalYearYearId",
                table: "Tombstones",
                column: "FiscalYearYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_FiscalYearYearId",
                table: "Risks",
                column: "FiscalYearYearId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorAndResults_FiscalYearYearId",
                table: "IndicatorAndResults",
                column: "FiscalYearYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndicatorAndResults_FiscalYears_FiscalYearYearId",
                table: "IndicatorAndResults",
                column: "FiscalYearYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_FiscalYears_FiscalYearYearId",
                table: "Risks",
                column: "FiscalYearYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tombstones_FiscalYears_FiscalYearYearId",
                table: "Tombstones",
                column: "FiscalYearYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndicatorAndResults_FiscalYears_FiscalYearYearId",
                table: "IndicatorAndResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Risks_FiscalYears_FiscalYearYearId",
                table: "Risks");

            migrationBuilder.DropForeignKey(
                name: "FK_Tombstones_FiscalYears_FiscalYearYearId",
                table: "Tombstones");

            migrationBuilder.DropIndex(
                name: "IX_Tombstones_FiscalYearYearId",
                table: "Tombstones");

            migrationBuilder.DropIndex(
                name: "IX_Risks_FiscalYearYearId",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_IndicatorAndResults_FiscalYearYearId",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "FiscalYearYearId",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "FiscalYearYearId",
                table: "Risks");

            migrationBuilder.DropColumn(
                name: "FiscalYearYearId",
                table: "IndicatorAndResults");
        }
    }
}

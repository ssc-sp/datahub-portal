using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class addnullablefks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "Tombstones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "Risks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "IndicatorAndResults",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tombstones_FiscalYearId",
                table: "Tombstones",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Risks_FiscalYearId",
                table: "Risks",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorAndResults_FiscalYearId",
                table: "IndicatorAndResults",
                column: "FiscalYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndicatorAndResults_FiscalYears_FiscalYearId",
                table: "IndicatorAndResults",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Risks_FiscalYears_FiscalYearId",
                table: "Risks",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tombstones_FiscalYears_FiscalYearId",
                table: "Tombstones",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId");
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

            migrationBuilder.DropIndex(
                name: "IX_Tombstones_FiscalYearId",
                table: "Tombstones");

            migrationBuilder.DropIndex(
                name: "IX_Risks_FiscalYearId",
                table: "Risks");

            migrationBuilder.DropIndex(
                name: "IX_IndicatorAndResults_FiscalYearId",
                table: "IndicatorAndResults");

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

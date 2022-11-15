using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class madeFKsNonNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearId",
                table: "Tombstones",
                type: "int",
                nullable: false,
                defaultValue: 2022,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearId",
                table: "Risks",
                type: "int",
                nullable: false,
                defaultValue: 2022,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearId",
                table: "IndicatorAndResults",
                type: "int",
                nullable: false,
                defaultValue: 2022,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearId",
                table: "Tombstones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearId",
                table: "Risks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "FiscalYearId",
                table: "IndicatorAndResults",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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
    }
}

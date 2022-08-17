using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class updatedfiscalyeartype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FiscalYear",
                table: "FundCenters",
                newName: "FiscalYearYearId");

            migrationBuilder.CreateIndex(
                name: "IX_FundCenters_FiscalYearYearId",
                table: "FundCenters",
                column: "FiscalYearYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_FundCenters_FiscalYears_FiscalYearYearId",
                table: "FundCenters",
                column: "FiscalYearYearId",
                principalTable: "FiscalYears",
                principalColumn: "YearId",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FundCenters_FiscalYears_FiscalYearYearId",
                table: "FundCenters");

            migrationBuilder.DropIndex(
                name: "IX_FundCenters_FiscalYearYearId",
                table: "FundCenters");

            migrationBuilder.RenameColumn(
                name: "FiscalYearYearId",
                table: "FundCenters",
                newName: "FiscalYear");
        }
    }
}

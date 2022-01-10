using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class renametargefields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Target_202021_DESC",
                table: "IndicatorAndResults",
                newName: "Target_DESC");

            migrationBuilder.RenameColumn(
                name: "Result_202021_DESC",
                table: "IndicatorAndResults",
                newName: "Result_DESC");

            migrationBuilder.RenameColumn(
                name: "Date_201920_Result_Collected_DT",
                table: "IndicatorAndResults",
                newName: "Date_Result_Collected");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Target_DESC",
                table: "IndicatorAndResults",
                newName: "Target_202021_DESC");

            migrationBuilder.RenameColumn(
                name: "Result_DESC",
                table: "IndicatorAndResults",
                newName: "Result_202021_DESC");

            migrationBuilder.RenameColumn(
                name: "Date_Result_Collected",
                table: "IndicatorAndResults",
                newName: "Date_201920_Result_Collected_DT");
        }
    }
}

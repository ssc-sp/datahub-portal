using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class NewFieldsForIndicator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Can_Report_On_Indicator",
                table: "IndicatorAndResults",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cannot_Report_On_Indicator",
                table: "IndicatorAndResults",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Does_Indicator_Enable_Program_Measure_Equity_Option",
                table: "IndicatorAndResults",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Can_Report_On_Indicator",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Cannot_Report_On_Indicator",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Does_Indicator_Enable_Program_Measure_Equity_Option",
                table: "IndicatorAndResults");
        }
    }
}

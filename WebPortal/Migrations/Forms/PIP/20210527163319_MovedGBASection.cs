using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations.Forms.PIP
{
    public partial class MovedGBASection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Does_Indicator_Enable_Program_Measure_Equity",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Does_Indicator_Enable_Program_Measure_Equity_Option",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Is_Equity_Seeking_Group",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Is_Equity_Seeking_Group2",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Is_Equity_Seeking_Group_Other",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "No_Equity_Seeking_Group",
                table: "IndicatorAndResults");

            migrationBuilder.AddColumn<string>(
                name: "Does_Indicator_Enable_Program_Measure_Equity",
                table: "Tombstones",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Does_Indicator_Enable_Program_Measure_Equity_Option",
                table: "Tombstones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Is_Equity_Seeking_Group",
                table: "Tombstones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Is_Equity_Seeking_Group2",
                table: "Tombstones",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Is_Equity_Seeking_Group_Other",
                table: "Tombstones",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "No_Equity_Seeking_Group",
                table: "Tombstones",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Does_Indicator_Enable_Program_Measure_Equity",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Does_Indicator_Enable_Program_Measure_Equity_Option",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Is_Equity_Seeking_Group",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Is_Equity_Seeking_Group2",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "Is_Equity_Seeking_Group_Other",
                table: "Tombstones");

            migrationBuilder.DropColumn(
                name: "No_Equity_Seeking_Group",
                table: "Tombstones");

            migrationBuilder.AddColumn<string>(
                name: "Does_Indicator_Enable_Program_Measure_Equity",
                table: "IndicatorAndResults",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Does_Indicator_Enable_Program_Measure_Equity_Option",
                table: "IndicatorAndResults",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Is_Equity_Seeking_Group",
                table: "IndicatorAndResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Is_Equity_Seeking_Group2",
                table: "IndicatorAndResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Is_Equity_Seeking_Group_Other",
                table: "IndicatorAndResults",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "No_Equity_Seeking_Group",
                table: "IndicatorAndResults",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}

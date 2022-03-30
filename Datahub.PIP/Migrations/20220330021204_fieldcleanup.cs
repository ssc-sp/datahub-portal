using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class fieldcleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Indicator__Progressive_Or_Aggregate_DESC",
                table: "IndicatorAndResults");

            migrationBuilder.DropColumn(
                name: "Tb_Sub_Indicator_Identification_Number_ID",
                table: "IndicatorAndResults");

            migrationBuilder.AddColumn<string>(
                name: "Trend_Rationale",
                table: "IndicatorAndResults",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Trend_Rationale",
                table: "IndicatorAndResults");

            migrationBuilder.AddColumn<string>(
                name: "Indicator__Progressive_Or_Aggregate_DESC",
                table: "IndicatorAndResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tb_Sub_Indicator_Identification_Number_ID",
                table: "IndicatorAndResults",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }
    }
}

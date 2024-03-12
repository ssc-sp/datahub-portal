using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Grossforecastfield : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "Total_Forecast",
				table: "SummaryForecasts",
				newName: "SFT_Forecast_Gross");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "SFT_Forecast_Gross",
				table: "SummaryForecasts",
				newName: "Total_Forecast");
		}
	}
}

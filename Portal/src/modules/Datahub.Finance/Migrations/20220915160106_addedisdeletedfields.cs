using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Addedisdeletedfields : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "Is_Deleted",
				table: "SummaryForecasts",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<bool>(
				name: "Is_Deleted",
				table: "Forecasts",
				type: "bit",
				nullable: false,
				defaultValue: false);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Is_Deleted",
				table: "SummaryForecasts");

			migrationBuilder.DropColumn(
				name: "Is_Deleted",
				table: "Forecasts");
		}
	}
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Newfields : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Key_Driver",
				table: "SummaryForecasts");

			migrationBuilder.AddColumn<string>(
				name: "Key_Activity_Additional_Information",
				table: "SummaryForecasts",
				type: "nvarchar(max)",
				maxLength: 5000,
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "FTE_Accomodations_Location",
				table: "Forecasts",
				type: "int",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "FTE_Accomodations_Requirements",
				table: "Forecasts",
				type: "int",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Position_Workspace_Type",
				table: "Forecasts",
				type: "int",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Key_Activity_Additional_Information",
				table: "SummaryForecasts");

			migrationBuilder.DropColumn(
				name: "FTE_Accomodations_Location",
				table: "Forecasts");

			migrationBuilder.DropColumn(
				name: "FTE_Accomodations_Requirements",
				table: "Forecasts");

			migrationBuilder.DropColumn(
				name: "Position_Workspace_Type",
				table: "Forecasts");

			migrationBuilder.AddColumn<int>(
				name: "Key_Driver",
				table: "SummaryForecasts",
				type: "int",
				nullable: true);
		}
	}
}

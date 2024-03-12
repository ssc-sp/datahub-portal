using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class FTEinSummary : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<double>(
				name: "FTE_Sum",
				table: "SummaryForecasts",
				type: "float",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "FTE_Sum",
				table: "SummaryForecasts");
		}
	}
}

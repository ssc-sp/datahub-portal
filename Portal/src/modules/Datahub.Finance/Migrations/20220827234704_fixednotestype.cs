using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Fixednotestype : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "AdditionalNotes",
				table: "SummaryForecasts",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(double),
				oldType: "float",
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<double>(
				name: "AdditionalNotes",
				table: "SummaryForecasts",
				type: "float",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);
		}
	}
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
	public partial class Isfiscalyearlockedfield : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "IsFiscalYearLocked",
				table: "IndicatorAndResults",
				type: "bit",
				nullable: false,
				defaultValue: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsFiscalYearLocked",
				table: "IndicatorAndResults");
		}
	}
}

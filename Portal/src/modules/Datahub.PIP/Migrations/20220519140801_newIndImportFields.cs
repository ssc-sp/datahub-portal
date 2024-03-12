using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
	public partial class NewIndImportFields : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "DataFactoryRunId",
				table: "IndicatorAndResults",
				type: "nvarchar(256)",
				maxLength: 256,
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "DuplicateCount",
				table: "IndicatorAndResults",
				type: "int",
				nullable: false,
				defaultValue: 0);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "DataFactoryRunId",
				table: "IndicatorAndResults");

			migrationBuilder.DropColumn(
				name: "DuplicateCount",
				table: "IndicatorAndResults");
		}
	}
}

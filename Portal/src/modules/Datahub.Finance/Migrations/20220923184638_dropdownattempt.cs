using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Dropdownattempt : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "Potential_Hiring_Process",
				table: "Forecasts",
				type: "int",
				maxLength: 4000,
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(string),
				oldType: "nvarchar(4000)",
				oldMaxLength: 4000,
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Potential_Hiring_Process",
				table: "Forecasts",
				type: "nvarchar(4000)",
				maxLength: 4000,
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int",
				oldMaxLength: 4000);
		}
	}
}

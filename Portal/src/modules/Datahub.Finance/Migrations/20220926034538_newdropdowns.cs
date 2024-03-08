using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Newdropdowns : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "Key_Driver",
				table: "SummaryForecasts",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(string),
				oldType: "nvarchar(100)",
				oldMaxLength: 100,
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "Key_Activity",
				table: "SummaryForecasts",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(string),
				oldType: "nvarchar(50)",
				oldMaxLength: 50,
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "Incremental_Replacement",
				table: "Forecasts",
				type: "int",
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
				name: "Key_Driver",
				table: "SummaryForecasts",
				type: "nvarchar(100)",
				maxLength: 100,
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<string>(
				name: "Key_Activity",
				table: "SummaryForecasts",
				type: "nvarchar(50)",
				maxLength: 50,
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<string>(
				name: "Incremental_Replacement",
				table: "Forecasts",
				type: "nvarchar(4000)",
				maxLength: 4000,
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");
		}
	}
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Madefieldsnullable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "Key_Driver",
				table: "SummaryForecasts",
				type: "int",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<int>(
				name: "Key_Activity",
				table: "SummaryForecasts",
				type: "int",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<int>(
				name: "Potential_Hiring_Process",
				table: "Forecasts",
				type: "int",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");

			migrationBuilder.AlterColumn<int>(
				name: "Incremental_Replacement",
				table: "Forecasts",
				type: "int",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "int");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<int>(
				name: "Key_Driver",
				table: "SummaryForecasts",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "Key_Activity",
				table: "SummaryForecasts",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "Potential_Hiring_Process",
				table: "Forecasts",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);

			migrationBuilder.AlterColumn<int>(
				name: "Incremental_Replacement",
				table: "Forecasts",
				type: "int",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "int",
				oldNullable: true);
		}
	}
}

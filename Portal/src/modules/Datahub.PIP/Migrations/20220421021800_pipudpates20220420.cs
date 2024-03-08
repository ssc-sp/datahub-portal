using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
	public partial class Pipudpates20220420 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Result_201718_DESC",
				table: "IndicatorAndResults");

			migrationBuilder.DropColumn(
				name: "Result_201819_DESC",
				table: "IndicatorAndResults");

			migrationBuilder.DropColumn(
				name: "Result_201920_DESC",
				table: "IndicatorAndResults");

			migrationBuilder.AddColumn<string>(
				name: "Transfer_Payment_Programs_8_DESC",
				table: "Tombstones",
				type: "nvarchar(400)",
				maxLength: 400,
				nullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Result_DESC",
				table: "IndicatorAndResults",
				type: "nvarchar(500)",
				maxLength: 500,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldMaxLength: 5000,
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Transfer_Payment_Programs_8_DESC",
				table: "Tombstones");

			migrationBuilder.AlterColumn<string>(
				name: "Result_DESC",
				table: "IndicatorAndResults",
				type: "nvarchar(max)",
				maxLength: 5000,
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(500)",
				oldMaxLength: 500,
				oldNullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Result_201718_DESC",
				table: "IndicatorAndResults",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Result_201819_DESC",
				table: "IndicatorAndResults",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Result_201920_DESC",
				table: "IndicatorAndResults",
				type: "nvarchar(1000)",
				maxLength: 1000,
				nullable: true);
		}
	}
}

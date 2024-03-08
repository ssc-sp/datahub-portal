using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations
{
	public partial class AddingQuarterNUMField : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<byte>(
				name: "Quarter_NUM",
				table: "LanguageTrainingApplications",
				type: "tinyint",
				nullable: false,
				defaultValue: (byte)0);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Quarter_NUM",
				table: "LanguageTrainingApplications");
		}
	}
}

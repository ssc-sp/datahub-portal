using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
	public partial class Makegcdocshyperlinkmandatory : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "GCdocs_Hyperlink_URL",
				table: "M365FormsApplications",
				type: "nvarchar(max)",
				nullable: false,
				defaultValue: "",
				oldClrType: typeof(string),
				oldType: "nvarchar(max)",
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "GCdocs_Hyperlink_URL",
				table: "M365FormsApplications",
				type: "nvarchar(max)",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "nvarchar(max)");
		}
	}
}

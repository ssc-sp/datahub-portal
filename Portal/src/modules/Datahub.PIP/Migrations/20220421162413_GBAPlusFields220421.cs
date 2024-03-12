using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
	public partial class GBAPlusFields220421 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "Collecting_Data",
				table: "Tombstones",
				type: "nvarchar(10)",
				maxLength: 10,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Disaggregated_Data",
				table: "Tombstones",
				type: "nvarchar(10)",
				maxLength: 10,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Disaggregated_Data_Information",
				table: "Tombstones",
				type: "nvarchar(4000)",
				maxLength: 4000,
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Collecting_Data",
				table: "Tombstones");

			migrationBuilder.DropColumn(
				name: "Disaggregated_Data",
				table: "Tombstones");

			migrationBuilder.DropColumn(
				name: "Disaggregated_Data_Information",
				table: "Tombstones");
		}
	}
}

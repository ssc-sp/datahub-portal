using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
	public partial class Programnoteslockedfield : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
				name: "IsProgramNotesLocked",
				table: "Tombstones",
				type: "bit",
				nullable: false,
				defaultValue: false);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "IsProgramNotesLocked",
				table: "Tombstones");
		}
	}
}

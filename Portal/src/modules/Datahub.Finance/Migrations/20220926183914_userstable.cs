using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Finance.Migrations
{
	public partial class Userstable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "BranchAccess",
				columns: table => new
				{
					BranchAccess_ID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					BranchFundCenter = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
					User = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
					IsInactive = table.Column<bool>(type: "bit", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_BranchAccess", x => x.BranchAccess_ID);
				});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "BranchAccess");
		}
	}
}

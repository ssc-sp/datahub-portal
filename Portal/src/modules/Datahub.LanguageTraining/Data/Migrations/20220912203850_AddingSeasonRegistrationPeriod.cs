using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations
{
	public partial class AddingSeasonRegistrationPeriod : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "SeasonRegistrationPeriods",
				columns: table => new
				{
					SeasonRegistrationPeriod_ID = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Year_NUM = table.Column<int>(type: "int", nullable: false),
					Quarter_NUM = table.Column<byte>(type: "tinyint", nullable: false),
					Open_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
					Close_DT = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SeasonRegistrationPeriods", x => x.SeasonRegistrationPeriod_ID);
				});

			migrationBuilder.CreateIndex(
				name: "IX_SeasonRegistrationPeriods_Year_NUM_Quarter_NUM",
				table: "SeasonRegistrationPeriods",
				columns: new[] { "Year_NUM", "Quarter_NUM" },
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SeasonRegistrationPeriods");
		}
	}
}

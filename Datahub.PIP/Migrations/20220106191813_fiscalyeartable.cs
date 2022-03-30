using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class fiscalyeartable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FiscalYears",
                columns: table => new
                {
                    YearId = table.Column<int>(type: "int", nullable: false),
                    FiscalYear = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYears", x => x.YearId);
                });

            migrationBuilder.InsertData(
                table: "FiscalYears",
                columns: new[] { "YearId", "FiscalYear" },
                values: new object[,]
                {
                    { 2018, "2017-18" },
                    { 2019, "2018-19" },
                    { 2020, "2019-20" },
                    { 2021, "2020-21" },
                    { 2022, "2021-22" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalYears");
        }
    }
}

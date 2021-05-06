using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class initialfinance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SectorAndBranches",
                columns: table => new
                {
                    SectorBranch_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sector_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch_TXT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch_Budget_NUM = table.Column<decimal>(type: "Money", nullable: true),
                    Allocated_Budget_NUM = table.Column<decimal>(type: "Money", nullable: true),
                    Unallocated_Budget_NUM = table.Column<decimal>(type: "Money", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectorAndBranches", x => x.SectorBranch_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectorAndBranches");
        }
    }
}

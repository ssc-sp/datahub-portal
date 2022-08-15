using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.Finance
{
    public partial class newfinancetables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FundCenters",
                columns: table => new
                {
                    FundCenter_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalYear = table.Column<int>(type: "int", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Division = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttritionRate = table.Column<double>(type: "float", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundCenters", x => x.FundCenter_ID);
                });

            migrationBuilder.CreateTable(
                name: "HierarchyLevels",
                columns: table => new
                {
                    HierarchyLevelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FCCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParentCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FundCenterNameEnglish = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FundCenterNameFrench = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HierarchyLevels", x => x.HierarchyLevelID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FundCenters");

            migrationBuilder.DropTable(
                name: "HierarchyLevels");
        }
    }
}

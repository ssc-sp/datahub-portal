using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class addedorganizationlevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeCreated",
                table: "Project_Resources",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "Organization_Levels",
                columns: table => new
                {
                    SectorAndBranchS_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Organization_ID = table.Column<int>(type: "int", nullable: false),
                    Full_Acronym_E = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Full_Acronym_F = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Acronym_E = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Acronym_F = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Name_E = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Name_F = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Org_Level = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Superior_OrgId = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Levels", x => x.SectorAndBranchS_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Organization_Levels");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeCreated",
                table: "Project_Resources",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}

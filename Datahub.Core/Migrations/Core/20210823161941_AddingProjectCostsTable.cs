using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class AddingProjectCostsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<DateTime>(
            //    name: "Requested_DT",
            //    table: "Project_Users_Requests",
            //    type: "datetime2",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Project_Costs",
                columns: table => new
                {
                    ProjectCosts_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Project_Acronym_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Usage_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost_AMT = table.Column<double>(type: "float", nullable: false),
                    Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Costs", x => x.ProjectCosts_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Costs");

            migrationBuilder.DropColumn(
                name: "Requested_DT",
                table: "Project_Users_Requests");
        }
    }
}

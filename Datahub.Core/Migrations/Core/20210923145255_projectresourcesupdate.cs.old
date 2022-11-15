using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class projectresourcesupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Param1",
                table: "Project_Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Param2",
                table: "Project_Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Param3",
                table: "Project_Resources",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Param1",
                table: "Project_Resources");

            migrationBuilder.DropColumn(
                name: "Param2",
                table: "Project_Resources");

            migrationBuilder.DropColumn(
                name: "Param3",
                table: "Project_Resources");
        }
    }
}

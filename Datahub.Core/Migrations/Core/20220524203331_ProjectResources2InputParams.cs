using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class ProjectResources2InputParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InputClassName",
                table: "Project_Resources2",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InputJsonContent",
                table: "Project_Resources2",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputClassName",
                table: "Project_Resources2");

            migrationBuilder.DropColumn(
                name: "InputJsonContent",
                table: "Project_Resources2");
        }
    }
}

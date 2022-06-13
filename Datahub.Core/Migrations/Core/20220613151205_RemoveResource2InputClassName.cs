using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class RemoveResource2InputClassName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InputClassName",
                table: "Project_Resources2");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InputClassName",
                table: "Project_Resources2",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}

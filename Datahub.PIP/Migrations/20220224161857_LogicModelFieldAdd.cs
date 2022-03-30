using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations.Forms.PIP
{
    public partial class LogicModelFieldAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Logic_Model",
                table: "Tombstones",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logic_Model",
                table: "Tombstones");
        }
    }
}

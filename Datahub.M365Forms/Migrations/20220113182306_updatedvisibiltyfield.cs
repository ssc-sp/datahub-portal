using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    public partial class updatedvisibiltyfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expected_Lifespan",
                table: "M365FormsApplications");

            migrationBuilder.AlterColumn<string>(
                name: "Visibility",
                table: "M365FormsApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Visibility",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Expected_Lifespan",
                table: "M365FormsApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

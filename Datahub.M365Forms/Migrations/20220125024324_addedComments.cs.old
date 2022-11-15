using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    public partial class addedComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "M365FormsApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "M365FormsApplications");
        }
    }
}

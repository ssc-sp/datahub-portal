using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    public partial class addednotificationfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationsSent",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationsSent",
                table: "M365FormsApplications");
        }
    }
}

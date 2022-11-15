using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.M365Forms.Migrations
{
    public partial class newfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Team_Purpose",
                table: "M365FormsApplications",
                newName: "Status");

            migrationBuilder.AddColumn<bool>(
                name: "Committee",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Event",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Ongoing_Lifespan",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Other",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Other_Txt",
                table: "M365FormsApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Or_Initiative",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Working_Group",
                table: "M365FormsApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Committee",
                table: "M365FormsApplications");

            migrationBuilder.DropColumn(
                name: "Event",
                table: "M365FormsApplications");

            migrationBuilder.DropColumn(
                name: "Ongoing_Lifespan",
                table: "M365FormsApplications");

            migrationBuilder.DropColumn(
                name: "Other",
                table: "M365FormsApplications");

            migrationBuilder.DropColumn(
                name: "Other_Txt",
                table: "M365FormsApplications");

            migrationBuilder.DropColumn(
                name: "Project_Or_Initiative",
                table: "M365FormsApplications");

            migrationBuilder.DropColumn(
                name: "Working_Group",
                table: "M365FormsApplications");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "M365FormsApplications",
                newName: "Team_Purpose");
        }
    }
}

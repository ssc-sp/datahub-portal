using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class moreuserfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Project_Users_Requests");

            migrationBuilder.AddColumn<string>(
                name: "Data_Sensitivity",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Is_Featured",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDataApprover",
                table: "Project_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Projects",
                keyColumn: "Project_ID",
                keyValue: 1,
                column: "Data_Sensitivity",
                value: "Unclassified");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data_Sensitivity",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Is_Featured",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsDataApprover",
                table: "Project_Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Project_Users_Requests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

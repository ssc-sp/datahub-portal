using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class AddRegistrationRequestFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedProjectAcronym",
                table: "Registration_Requests",
                newName: "ProjectAcronym");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Registration_Requests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Registration_Requests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Registration_Requests");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Registration_Requests");

            migrationBuilder.RenameColumn(
                name: "ProjectAcronym",
                table: "Registration_Requests",
                newName: "RequestedProjectAcronym");
        }
    }
}

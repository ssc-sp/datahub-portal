using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Shared.Migrations.Core
{
    public partial class addedprojectname : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Project_Name",
                table: "OnboardingApps",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Project_Name",
                table: "OnboardingApps");
        }
    }
}

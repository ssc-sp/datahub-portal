using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class languagefields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Client_Branch_EN",
                table: "OnboardingApps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Branch_FR",
                table: "OnboardingApps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Division_EN",
                table: "OnboardingApps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Division_FR",
                table: "OnboardingApps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Sector_EN",
                table: "OnboardingApps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Sector_FR",
                table: "OnboardingApps",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Client_Branch_EN",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Client_Branch_FR",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Client_Division_EN",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Client_Division_FR",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Client_Sector_EN",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Client_Sector_FR",
                table: "OnboardingApps");
        }
    }
}

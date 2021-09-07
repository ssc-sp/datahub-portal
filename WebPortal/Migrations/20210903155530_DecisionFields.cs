using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class DecisionFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplicationCompleteEmailSent",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LSUDecisionSent",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ManagerDecisionSent",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Manager_Decision",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationCompleteEmailSent",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "LSUDecisionSent",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "ManagerDecisionSent",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Manager_Decision",
                table: "LanguageTrainingApplications");
        }
    }
}

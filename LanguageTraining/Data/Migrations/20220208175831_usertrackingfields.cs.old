using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Portal.Migrations
{
    public partial class usertrackingfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageClass",
                table: "LanguageTrainingApplications");

            migrationBuilder.AddColumn<DateTime>(
                name: "FormSubmitted_DT",
                table: "LanguageTrainingApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FormSubmitted_UserId",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LanguageSchoolDecision_DT",
                table: "LanguageTrainingApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LanguageSchoolDecision_UserId",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManagerDecision_DT",
                table: "LanguageTrainingApplications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ManagerDecision_UserId",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormSubmitted_DT",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "FormSubmitted_UserId",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "LanguageSchoolDecision_DT",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "LanguageSchoolDecision_UserId",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "ManagerDecision_DT",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "ManagerDecision_UserId",
                table: "LanguageTrainingApplications");

            migrationBuilder.AddColumn<string>(
                name: "LanguageClass",
                table: "LanguageTrainingApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}

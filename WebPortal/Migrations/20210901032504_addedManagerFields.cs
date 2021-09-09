using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class addedManagerFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SLE_Results_Writing",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SLE_Results_Reading",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SLE_Results_Oral",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Last_Course_Successfully_Completed",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Language_Training_Provided_By",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Completed_Training_Session",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Decision",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Employee_Appointed_NonImperative_Basis",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Employee_equity_group",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Employee_language_profile_raised",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Employee_professional_dev_program",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Employee_talent_management_exercise",
                table: "LanguageTrainingApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Manager_Email_Address",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manager_First_Name",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manager_Last_Name",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Decision",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Employee_Appointed_NonImperative_Basis",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Employee_equity_group",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Employee_language_profile_raised",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Employee_professional_dev_program",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Employee_talent_management_exercise",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Manager_Email_Address",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Manager_First_Name",
                table: "LanguageTrainingApplications");

            migrationBuilder.DropColumn(
                name: "Manager_Last_Name",
                table: "LanguageTrainingApplications");

            migrationBuilder.AlterColumn<string>(
                name: "SLE_Results_Writing",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SLE_Results_Reading",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SLE_Results_Oral",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Last_Course_Successfully_Completed",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language_Training_Provided_By",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Completed_Training_Session",
                table: "LanguageTrainingApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}

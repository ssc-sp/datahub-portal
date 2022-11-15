using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class onboardingupdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data_Set_Security_Level_Classified",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Data_Set_Security_Level_ProtectedA",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Data_Set_Security_Level_ProtectedB",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Data_Set_Security_Level_ProtectedC",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Data_Set_Security_Level_UnClassified",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_DataPipeline",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_DataScience",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_FullStack",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_Guidance",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_Other",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_OtherText",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_PowerBIReports",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_Storage",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_Unknown",
                table: "OnboardingApps");

            migrationBuilder.DropColumn(
                name: "Project_Engagement_Category_WebForms",
                table: "OnboardingApps");

            migrationBuilder.RenameColumn(
                name: "Attachments",
                table: "OnboardingApps",
                newName: "Project_Engagement_Category");

            migrationBuilder.AddColumn<string>(
                name: "Data_Security_Level",
                table: "OnboardingApps",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data_Security_Level",
                table: "OnboardingApps");

            migrationBuilder.RenameColumn(
                name: "Project_Engagement_Category",
                table: "OnboardingApps",
                newName: "Attachments");

            migrationBuilder.AddColumn<bool>(
                name: "Data_Set_Security_Level_Classified",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Data_Set_Security_Level_ProtectedA",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Data_Set_Security_Level_ProtectedB",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Data_Set_Security_Level_ProtectedC",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Data_Set_Security_Level_UnClassified",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_DataPipeline",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_DataScience",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_FullStack",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_Guidance",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_Other",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Project_Engagement_Category_OtherText",
                table: "OnboardingApps",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_PowerBIReports",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_Storage",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_Unknown",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Project_Engagement_Category_WebForms",
                table: "OnboardingApps",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

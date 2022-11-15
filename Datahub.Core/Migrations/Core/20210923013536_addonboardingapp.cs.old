using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class addonboardingapp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingApps",
                columns: table => new
                {
                    Application_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_Sector = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Client_Branch = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Client_Division = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Client_Contact_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Client_Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Additional_Contact_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Additional_Contact_Email_EMAIL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Project_Summary_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Goal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Onboarding_Timeline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Engagement_Category_DataPipeline = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_DataScience = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_FullStack = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_Guidance = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_PowerBIReports = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_Storage = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_WebForms = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_Unknown = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_Other = table.Column<bool>(type: "bit", nullable: false),
                    Project_Engagement_Category_OtherText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Data_Set_Security_Level_Classified = table.Column<bool>(type: "bit", nullable: false),
                    Data_Set_Security_Level_ProtectedA = table.Column<bool>(type: "bit", nullable: false),
                    Data_Set_Security_Level_ProtectedB = table.Column<bool>(type: "bit", nullable: false),
                    Data_Set_Security_Level_ProtectedC = table.Column<bool>(type: "bit", nullable: false),
                    Data_Set_Security_Level_UnClassified = table.Column<bool>(type: "bit", nullable: false),
                    Questions_for_the_DataHub_Team = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingApps", x => x.Application_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnboardingApps");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LanguageTrainingApplications",
                columns: table => new
                {
                    Application_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NRCan_Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    First_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email_Address_EMAIL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sector_Branch = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Province_Territory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Employment_Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    I_am_seeking = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Completed_LETP_Assessment = table.Column<bool>(type: "bit", nullable: false),
                    Language_Training_Since_LETP_Assessment = table.Column<bool>(type: "bit", nullable: false),
                    Language_Training_Provided_By = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_Course_Successfully_Completed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Completed_Training_Year = table.Column<int>(type: "int", nullable: false),
                    Completed_Training_Session = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Report_Sent_To_NRCan_Language_School = table.Column<bool>(type: "bit", nullable: false),
                    Second_Language_Evaluation_Results = table.Column<bool>(type: "bit", nullable: false),
                    SLE_Results_Reading = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SLE_Results_Writing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SLE_Results_Oral = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Training_Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SLE_Test_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Session_For_Language_Training = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Class_For_Language_Training = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Delegate_Manager_First_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delegated_Manager_Last_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Delegated_Manager_Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageTrainingApplications", x => x.Application_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageTrainingApplications");
        }
    }
}

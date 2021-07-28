using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class ServiceRequestingUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Notification_Sent",
                table: "Project_Requests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_ID",
                table: "Project_Requests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_Name",
                table: "Project_Requests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // migrationBuilder.InsertData(
            //     table: "Projects",
            //     columns: new[] { "Project_ID", "Branch_Name", "Comments_NT", "Contact_List", "Databricks_URL", "Deleted_DT", "Division_Name", "GC_Docs_URL", "Initial_Meeting_DT", "Is_Private", "Last_Contact_DT", "Last_Updated_DT", "Next_Meeting_DT", "Number_Of_Users_Involved", "PowerBI_URL", "Project_Acronym_CD", "Project_Admin", "Project_Category", "Project_Icon", "Project_Name", "Project_Name_Fr", "Project_Phase", "Project_Status_Desc", "Project_Summary_Desc", "Project_Summary_Desc_Fr", "Sector_Name", "Stage_Desc", "WebForms_URL" },
            //     values: new object[] { 1, null, null, null, null, null, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "DHTRK", null, null, "database", "Datahub Projects", null, null, "Ongoing", "Datahub Project Tracker", null, "CIOSB", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DeleteData(
            //     table: "Projects",
            //     keyColumn: "Project_ID",
            //     keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Notification_Sent",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "User_ID",
                table: "Project_Requests");

            migrationBuilder.DropColumn(
                name: "User_Name",
                table: "Project_Requests");
        }
    }
}

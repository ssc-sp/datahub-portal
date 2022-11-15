using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class userrequesttable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Databricks",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "PowerBI",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "User_Name",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "WebForms",
                table: "Project_Users");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedUser",
                table: "Project_Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Approved_DT",
                table: "Project_Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Project_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Project_Users_Requests",
                columns: table => new
                {
                    ProjectUserRequest_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Approved_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Users_Requests", x => x.ProjectUserRequest_ID);
                    table.ForeignKey(
                        name: "FK_Project_Users_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            //migrationBuilder.InsertData(
            //    table: "Projects",
            //    columns: new[] { "Project_ID", "Branch_Name", "Comments_NT", "Contact_List", "Databricks_URL", "Deleted_DT", "Division_Name", "GC_Docs_URL", "Initial_Meeting_DT", "Is_Private", "Last_Contact_DT", "Last_Updated_DT", "Next_Meeting_DT", "Number_Of_Users_Involved", "PowerBI_URL", "Project_Acronym_CD", "Project_Admin", "Project_Category", "Project_Icon", "Project_Name", "Project_Name_Fr", "Project_Phase", "Project_Status_Desc", "Project_Summary_Desc", "Project_Summary_Desc_Fr", "Sector_Name", "Stage_Desc", "WebForms_URL" },
            //    values: new object[] { 1, null, null, null, null, null, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "DHTRK", null, null, "database", "Datahub Projects", null, null, "Ongoing", "Datahub Project Tracker", null, "CIOSB", null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Requests_Project_ID",
                table: "Project_Users_Requests",
                column: "Project_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Users_Requests");

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "Project_ID",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "ApprovedUser",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "Approved_DT",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Project_Users");

            migrationBuilder.AddColumn<bool>(
                name: "Databricks",
                table: "Project_Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PowerBI",
                table: "Project_Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_Name",
                table: "Project_Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "WebForms",
                table: "Project_Users",
                type: "bit",
                nullable: true);
        }
    }
}

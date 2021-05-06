using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.ProjectForms.Migrations.DatahubProjectDB
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sector_Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Branch_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Division_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contact_List = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Project_Acronym_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Project_Summary_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Initial_Meeting_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Stage_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Status_Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GC_Docs_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments_NT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Contact_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Next_Meeting_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Project_ID);
                });

            migrationBuilder.CreateTable(
                name: "PowerBI_License_Requests",
                columns: table => new
                {
                    Request_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Premium_License_Flag = table.Column<bool>(type: "bit", nullable: false),
                    Contact_Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contact_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Desktop_Usage_Flag = table.Column<bool>(type: "bit", nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBI_License_Requests", x => x.Request_ID);
                    table.ForeignKey(
                        name: "FK_PowerBI_License_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project_Comments",
                columns: table => new
                {
                    Comment_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Comment_Date_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment_NT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Comments", x => x.Comment_ID);
                    table.ForeignKey(
                        name: "FK_Project_Comments_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowerBI_License_User_Requests",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBI_License_User_Requests", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PowerBI_License_User_Requests_PowerBI_License_Requests_RequestID",
                        column: x => x.RequestID,
                        principalTable: "PowerBI_License_Requests",
                        principalColumn: "Request_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerBI_License_Requests_Project_ID",
                table: "PowerBI_License_Requests",
                column: "Project_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerBI_License_User_Requests_RequestID",
                table: "PowerBI_License_User_Requests",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Comments_Project_ID",
                table: "Project_Comments",
                column: "Project_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerBI_License_User_Requests");

            migrationBuilder.DropTable(
                name: "Project_Comments");

            migrationBuilder.DropTable(
                name: "PowerBI_License_Requests");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}

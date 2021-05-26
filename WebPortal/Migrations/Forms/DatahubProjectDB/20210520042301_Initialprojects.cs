using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NRCan.Datahub.Portal.Migrations.Forms.DatahubProjectDB
{
    public partial class Initialprojects : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DBCodes",
                columns: table => new
                {
                    DBCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ClassWord_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClassWord_DEF = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBCodes", x => x.DBCode);
                });

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
                    Project_Name_Fr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Project_Acronym_CD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Project_Admin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Summary_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Summary_Desc_Fr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Initial_Meeting_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Number_Of_Users_Involved = table.Column<int>(type: "int", nullable: true),
                    Is_Private = table.Column<bool>(type: "bit", nullable: false),
                    Stage_Desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Status_Desc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Project_Phase = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GC_Docs_URL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments_NT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Last_Contact_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Next_Meeting_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Databricks_URL = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PowerBI_URL = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    WebForms_URL = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Project_ID);
                });

            migrationBuilder.CreateTable(
                name: "Access_Requests",
                columns: table => new
                {
                    Request_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Databricks = table.Column<bool>(type: "bit", nullable: false),
                    PowerBI = table.Column<bool>(type: "bit", nullable: false),
                    WebForms = table.Column<bool>(type: "bit", nullable: false),
                    Request_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Completion_DT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Access_Requests", x => x.Request_ID);
                    table.ForeignKey(
                        name: "FK_Access_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Project_Pipeline_Links",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "int", nullable: false),
                    Process_Nm = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Pipeline_Links", x => new { x.Project_ID, x.Process_Nm });
                    table.ForeignKey(
                        name: "FK_Project_Pipeline_Links_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Project_Requests",
                columns: table => new
                {
                    ServiceRequests_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceRequests_Date_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Is_Completed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Requests", x => x.ServiceRequests_ID);
                    table.ForeignKey(
                        name: "FK_Project_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project_Users",
                columns: table => new
                {
                    ProjectUser_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    User_ID = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Databricks = table.Column<bool>(type: "bit", nullable: true),
                    PowerBI = table.Column<bool>(type: "bit", nullable: true),
                    WebForms = table.Column<bool>(type: "bit", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Users", x => x.ProjectUser_ID);
                    table.ForeignKey(
                        name: "FK_Project_Users_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WebForms",
                columns: table => new
                {
                    WebForm_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Project_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebForms", x => x.WebForm_ID);
                    table.ForeignKey(
                        name: "FK_WebForms_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    FieldID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Section_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Field_DESC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description_DESC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Extension_CD = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "NONE"),
                    Type_CD = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "Text"),
                    Max_Length_NUM = table.Column<int>(type: "int", nullable: true),
                    Notes_TXT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mandatory_FLAG = table.Column<bool>(type: "bit", nullable: false),
                    Date_Updated_DT = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WebForm_ID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fields", x => x.FieldID);
                    table.ForeignKey(
                        name: "FK_Fields_WebForms_WebForm_ID",
                        column: x => x.WebForm_ID,
                        principalTable: "WebForms",
                        principalColumn: "WebForm_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Access_Requests_Project_ID",
                table: "Access_Requests",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_WebForm_ID",
                table: "Fields",
                column: "WebForm_ID");

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

            migrationBuilder.CreateIndex(
                name: "IX_Project_Requests_Project_ID",
                table: "Project_Requests",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Project_ID",
                table: "Project_Users",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WebForms_Project_ID",
                table: "WebForms",
                column: "Project_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Access_Requests");

            migrationBuilder.DropTable(
                name: "DBCodes");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "PowerBI_License_User_Requests");

            migrationBuilder.DropTable(
                name: "Project_Comments");

            migrationBuilder.DropTable(
                name: "Project_Pipeline_Links");

            migrationBuilder.DropTable(
                name: "Project_Requests");

            migrationBuilder.DropTable(
                name: "Project_Users");

            migrationBuilder.DropTable(
                name: "WebForms");

            migrationBuilder.DropTable(
                name: "PowerBI_License_Requests");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}

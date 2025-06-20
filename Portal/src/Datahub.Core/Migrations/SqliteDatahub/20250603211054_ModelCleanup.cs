using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class ModelCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_BranchId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_DivisionId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_SectorId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Client_Engagements");

            migrationBuilder.DropTable(
                name: "DBCodes");

            migrationBuilder.DropTable(
                name: "ExternalPowerBiReports");

            migrationBuilder.DropTable(
                name: "Fields");

            migrationBuilder.DropTable(
                name: "PowerBi_DataSets");

            migrationBuilder.DropTable(
                name: "PowerBI_License_User_Requests");

            migrationBuilder.DropTable(
                name: "PowerBi_Reports");

            migrationBuilder.DropTable(
                name: "Project_Comments");

            migrationBuilder.DropTable(
                name: "Project_Pipeline_Links");

            migrationBuilder.DropTable(
                name: "Project_Users_Requests");

            migrationBuilder.DropTable(
                name: "WebForms");

            migrationBuilder.DropTable(
                name: "PowerBI_License_Requests");

            migrationBuilder.DropTable(
                name: "PowerBi_Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Projects_BranchId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Branch_Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Comments_NT",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DB_Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DB_Server",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Databricks_URL",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Division_Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "GC_Docs_URL",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "HasCostRecovery",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Initial_Meeting_DT",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Last_Contact_DT",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Next_Meeting_DT",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "OnboardingApplicationId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PowerBI_URL",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Project_Category",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Project_Goal",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Sector_Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Stage_Desc",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WebForms_URL",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AllowVMs",
                table: "Project_Whitelists");

            migrationBuilder.DropColumn(
                name: "ApprovedUser",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "IsDataApprover",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "User_ID",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "User_Name",
                table: "Project_Users");

            migrationBuilder.RenameColumn(
                name: "SectorId",
                table: "Projects",
                newName: "Organization_LevelSectorAndBranchS_ID2");

            migrationBuilder.RenameColumn(
                name: "Number_Of_Users_Involved",
                table: "Projects",
                newName: "Organization_LevelSectorAndBranchS_ID1");

            migrationBuilder.RenameColumn(
                name: "DivisionId",
                table: "Projects",
                newName: "Organization_LevelSectorAndBranchS_ID");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_SectorId",
                table: "Projects",
                newName: "IX_Projects_Organization_LevelSectorAndBranchS_ID2");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_DivisionId",
                table: "Projects",
                newName: "IX_Projects_Organization_LevelSectorAndBranchS_ID");

            migrationBuilder.CreateTable(
                name: "PortalUserStatusChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PortalUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortalUserStatusChanges", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Project_Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 6, "A user whose access has been disabled and cannot interact with the workspace", "Disabled User" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID1",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_Organization_LevelSectorAndBranchS_ID",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_Organization_LevelSectorAndBranchS_ID1",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID1",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_Organization_LevelSectorAndBranchS_ID2",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID2",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_Organization_LevelSectorAndBranchS_ID",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_Organization_LevelSectorAndBranchS_ID1",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Organization_Levels_Organization_LevelSectorAndBranchS_ID2",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "PortalUserStatusChanges");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID1",
                table: "Projects");

            migrationBuilder.DeleteData(
                table: "Project_Roles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.RenameColumn(
                name: "Organization_LevelSectorAndBranchS_ID2",
                table: "Projects",
                newName: "SectorId");

            migrationBuilder.RenameColumn(
                name: "Organization_LevelSectorAndBranchS_ID1",
                table: "Projects",
                newName: "Number_Of_Users_Involved");

            migrationBuilder.RenameColumn(
                name: "Organization_LevelSectorAndBranchS_ID",
                table: "Projects",
                newName: "DivisionId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID2",
                table: "Projects",
                newName: "IX_Projects_SectorId");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID",
                table: "Projects",
                newName: "IX_Projects_DivisionId");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Branch_Name",
                table: "Projects",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comments_NT",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DB_Name",
                table: "Projects",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DB_Server",
                table: "Projects",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Databricks_URL",
                table: "Projects",
                type: "TEXT",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Division_Name",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GC_Docs_URL",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasCostRecovery",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Initial_Meeting_DT",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Last_Contact_DT",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Next_Meeting_DT",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnboardingApplicationId",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PowerBI_URL",
                table: "Projects",
                type: "TEXT",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Project_Category",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Project_Goal",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector_Name",
                table: "Projects",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage_Desc",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebForms_URL",
                table: "Projects",
                type: "TEXT",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowVMs",
                table: "Project_Whitelists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedUser",
                table: "Project_Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Project_Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDataApprover",
                table: "Project_Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "User_ID",
                table: "Project_Users",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "User_Name",
                table: "Project_Users",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Client_Engagements",
                columns: table => new
                {
                    Engagement_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    Actual_Release_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Created_UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Engagement_Name = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Engagement_Start_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Engagment_Lead = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Engagment_Owners = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Final_Release_Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Final_Updates_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Is_Engagement_Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    Last_Updated_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Phase1_Development_ActualEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase1_Development_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase1_Testing_ActualEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase1_Testing_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase2_Development_ActualEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase2_Development_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase2_Testing_ActualEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Phase2_Testing_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Requirements_Gathering_ActualEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Requirements_Gathering_EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client_Engagements", x => x.Engagement_ID);
                    table.ForeignKey(
                        name: "FK_Client_Engagements_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "DBCodes",
                columns: table => new
                {
                    DBCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ClassWord_DEF = table.Column<string>(type: "TEXT", nullable: false),
                    ClassWord_DESC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBCodes", x => x.DBCode);
                });

            migrationBuilder.CreateTable(
                name: "ExternalPowerBiReports",
                columns: table => new
                {
                    ExternalPowerBiReport_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    End_Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Is_Created = table.Column<bool>(type: "INTEGER", nullable: false),
                    Report_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestingUser = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Token = table.Column<string>(type: "TEXT", nullable: true),
                    Url = table.Column<string>(type: "TEXT", nullable: true),
                    ValidationSalt = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Validation_Code = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalPowerBiReports", x => x.ExternalPowerBiReport_ID);
                });

            migrationBuilder.CreateTable(
                name: "PowerBI_License_Requests",
                columns: table => new
                {
                    Request_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Contact_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Contact_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Desktop_Usage_Flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    Premium_License_Flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    User_ID = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "PowerBi_Workspaces",
                columns: table => new
                {
                    Workspace_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Project_Id = table.Column<int>(type: "INTEGER", nullable: true),
                    Sandbox_Flag = table.Column<bool>(type: "INTEGER", nullable: false),
                    Workspace_Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBi_Workspaces", x => x.Workspace_ID);
                    table.ForeignKey(
                        name: "FK_PowerBi_Workspaces_Projects_Project_Id",
                        column: x => x.Project_Id,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Comments",
                columns: table => new
                {
                    Comment_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    Comment_Date_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Comment_NT = table.Column<string>(type: "TEXT", nullable: true),
                    Created_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Created_UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Comments", x => x.Comment_ID);
                    table.ForeignKey(
                        name: "FK_Project_Comments_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_Pipeline_Links",
                columns: table => new
                {
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Process_Nm = table.Column<string>(type: "TEXT", nullable: false)
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
                name: "Project_Users_Requests",
                columns: table => new
                {
                    ProjectUserRequest_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: true),
                    ApprovedUser = table.Column<string>(type: "TEXT", nullable: true),
                    Approved_DT = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Requested_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    User_ID = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Users_Requests", x => x.ProjectUserRequest_ID);
                    table.ForeignKey(
                        name: "FK_Project_Users_Requests_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "WebForms",
                columns: table => new
                {
                    WebForm_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Project_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Description_DESC = table.Column<string>(type: "TEXT", nullable: true),
                    Title_DESC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
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
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RequestID = table.Column<int>(type: "INTEGER", nullable: false),
                    LicenseType = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    UserEmail = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
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
                name: "PowerBi_DataSets",
                columns: table => new
                {
                    DataSet_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Workspace_Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataSet_Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBi_DataSets", x => x.DataSet_ID);
                    table.ForeignKey(
                        name: "FK_PowerBi_DataSets_PowerBi_Workspaces_Workspace_Id",
                        column: x => x.Workspace_Id,
                        principalTable: "PowerBi_Workspaces",
                        principalColumn: "Workspace_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerBi_Reports",
                columns: table => new
                {
                    Report_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Workspace_Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InCatalog = table.Column<bool>(type: "INTEGER", nullable: false),
                    Report_Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerBi_Reports", x => x.Report_ID);
                    table.ForeignKey(
                        name: "FK_PowerBi_Reports_PowerBi_Workspaces_Workspace_Id",
                        column: x => x.Workspace_Id,
                        principalTable: "PowerBi_Workspaces",
                        principalColumn: "Workspace_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fields",
                columns: table => new
                {
                    FieldID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WebForm_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Choices_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Date_Updated_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description_DESC = table.Column<string>(type: "TEXT", nullable: true),
                    Extension_CD = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false, defaultValue: "NONE"),
                    Field_DESC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Mandatory_FLAG = table.Column<bool>(type: "INTEGER", nullable: false),
                    Max_Length_NUM = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes_TXT = table.Column<string>(type: "TEXT", nullable: true),
                    Section_DESC = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Type_CD = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false, defaultValue: "Text")
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
                name: "IX_Projects_BranchId",
                table: "Projects",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_Engagements_Project_ID",
                table: "Client_Engagements",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Fields_WebForm_ID",
                table: "Fields",
                column: "WebForm_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_DataSets_Workspace_Id",
                table: "PowerBi_DataSets",
                column: "Workspace_Id");

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
                name: "IX_PowerBi_Reports_Workspace_Id",
                table: "PowerBi_Reports",
                column: "Workspace_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_Workspaces_Project_Id",
                table: "PowerBi_Workspaces",
                column: "Project_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Comments_Project_ID",
                table: "Project_Comments",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_Requests_Project_ID",
                table: "Project_Users_Requests",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_WebForms_Project_ID",
                table: "WebForms",
                column: "Project_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_BranchId",
                table: "Projects",
                column: "BranchId",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_DivisionId",
                table: "Projects",
                column: "DivisionId",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Organization_Levels_SectorId",
                table: "Projects",
                column: "SectorId",
                principalTable: "Organization_Levels",
                principalColumn: "SectorAndBranchS_ID");
        }
    }
}

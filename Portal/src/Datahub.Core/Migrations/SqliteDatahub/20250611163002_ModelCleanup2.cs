using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class ModelCleanup2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "OnboardingApps");

            migrationBuilder.DropTable(
                name: "Organization_Levels");

            migrationBuilder.DropTable(
                name: "PublicDataFiles");

            migrationBuilder.DropTable(
                name: "SpatialObjectShares");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID1",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID2",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Organization_LevelSectorAndBranchS_ID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Organization_LevelSectorAndBranchS_ID1",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Organization_LevelSectorAndBranchS_ID2",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "PortalUserStatusChanges",
                newName: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "PortalUserStatusChanges",
                newName: "StatusId");

            migrationBuilder.AddColumn<int>(
                name: "Organization_LevelSectorAndBranchS_ID",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Organization_LevelSectorAndBranchS_ID1",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Organization_LevelSectorAndBranchS_ID2",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OnboardingApps",
                columns: table => new
                {
                    Application_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Additional_Contact_Email_EMAIL = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Additional_Contact_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Client_Branch = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Client_Contact_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Client_Division = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Client_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Client_Sector = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Data_Security_Level = table.Column<string>(type: "TEXT", nullable: true),
                    Last_Updated_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Last_Updated_UserId = table.Column<string>(type: "TEXT", nullable: true),
                    NotificationsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Onboarding_Timeline = table.Column<string>(type: "TEXT", nullable: true),
                    Product_Name = table.Column<string>(type: "TEXT", nullable: true),
                    ProjectCreatedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Project_Engagement_Category = table.Column<string>(type: "TEXT", nullable: true),
                    Project_Engagement_Category_Other = table.Column<string>(type: "TEXT", nullable: true),
                    Project_Goal = table.Column<string>(type: "TEXT", nullable: true),
                    Project_Summary_Description = table.Column<string>(type: "TEXT", nullable: true),
                    Questions_for_the_DataHub_Team = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingApps", x => x.Application_ID);
                });

            migrationBuilder.CreateTable(
                name: "Organization_Levels",
                columns: table => new
                {
                    SectorAndBranchS_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Full_Acronym_E = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Full_Acronym_F = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Org_Acronym_E = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Org_Acronym_F = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Org_Level = table.Column<string>(type: "TEXT", maxLength: 1, nullable: true),
                    Org_Name_E = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Org_Name_F = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    Organization_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Superior_OrgId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization_Levels", x => x.SectorAndBranchS_ID);
                });

            migrationBuilder.CreateTable(
                name: "PublicDataFiles",
                columns: table => new
                {
                    PublicDataFile_ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApprovedDate_DT = table.Column<DateTime>(type: "TEXT", nullable: true),
                    File_ID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Filename_TXT = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FolderPath_TXT = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    ProjectCode_CD = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    PublicationDate_DT = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RequestedDate_DT = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestingUser_ID = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SubmittedDate_DT = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicDataFiles", x => x.PublicDataFile_ID);
                });

            migrationBuilder.CreateTable(
                name: "SpatialObjectShares",
                columns: table => new
                {
                    GeoObjectShare_ID = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    ApprovalForm_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Approval_Document_URL = table.Column<string>(type: "TEXT", nullable: true),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Email_Contact_TXT = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Json_TXT = table.Column<string>(type: "TEXT", nullable: false),
                    Publication_ID = table.Column<string>(type: "TEXT", nullable: true),
                    ShareStatus = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpatialObjectShares", x => x.GeoObjectShare_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID1",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID1");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Organization_LevelSectorAndBranchS_ID2",
                table: "Projects",
                column: "Organization_LevelSectorAndBranchS_ID2");

            migrationBuilder.CreateIndex(
                name: "IX_PublicDataFiles_File_ID",
                table: "PublicDataFiles",
                column: "File_ID",
                unique: true);

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
    }
}

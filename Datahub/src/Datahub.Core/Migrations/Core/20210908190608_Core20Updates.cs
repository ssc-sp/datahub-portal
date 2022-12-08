using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Migrations.Core
{
    public partial class Core20Updates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project_Databases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Databases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Databases_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project_PBI_DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Workspace = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    DatasetName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_PBI_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_PBI_DataSets_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project_PBI_Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    Workspace = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_PBI_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_PBI_Reports_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project_PBI_Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_PBI_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_PBI_Workspaces_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Project_Storage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Storage_Type = table.Column<int>(type: "int", nullable: false),
                    Datahub_ProjectProject_ID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_Storage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_Storage_Projects_Datahub_ProjectProject_ID",
                        column: x => x.Datahub_ProjectProject_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_Databases_Project_ID",
                table: "Project_Databases",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_DataSets_Project_ID",
                table: "Project_PBI_DataSets",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_Reports_Project_ID",
                table: "Project_PBI_Reports",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_Workspaces_Project_ID",
                table: "Project_PBI_Workspaces",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Storage_Datahub_ProjectProject_ID",
                table: "Project_Storage",
                column: "Datahub_ProjectProject_ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_Databases");

            migrationBuilder.DropTable(
                name: "Project_PBI_DataSets");

            migrationBuilder.DropTable(
                name: "Project_PBI_Reports");

            migrationBuilder.DropTable(
                name: "Project_PBI_Workspaces");

            migrationBuilder.DropTable(
                name: "Project_Storage");

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Project_ID", "Branch_Name", "Comments_NT", "Contact_List", "Data_Sensitivity", "Databricks_URL", "Deleted_DT", "Division_Name", "GC_Docs_URL", "Initial_Meeting_DT", "Is_Featured", "Is_Private", "Last_Contact_DT", "Last_Updated_DT", "Next_Meeting_DT", "Number_Of_Users_Involved", "PowerBI_URL", "Project_Acronym_CD", "Project_Admin", "Project_Category", "Project_Icon", "Project_Name", "Project_Name_Fr", "Project_Phase", "Project_Status_Desc", "Project_Summary_Desc", "Project_Summary_Desc_Fr", "Sector_Name", "Stage_Desc", "WebForms_URL" },
                values: new object[] { 1, null, null, null, "Unclassified", null, null, null, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, false, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "DHTRK", null, null, "database", "Datahub Projects", null, null, "Ongoing", "Datahub Project Tracker", null, "CIOSB", null, null });
        }
    }
}

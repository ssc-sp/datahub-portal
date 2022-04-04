using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class RemovingPBIUnusedTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Project_PBI_DataSets");

            migrationBuilder.DropTable(
                name: "Project_PBI_Reports");

            migrationBuilder.DropTable(
                name: "Project_PBI_Workspaces");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Project_PBI_DataSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    DatasetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Workspace = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_PBI_DataSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_PBI_DataSets_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_PBI_Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    WorkspaceName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_PBI_Workspaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_PBI_Workspaces_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateTable(
                name: "Project_PBI_Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Project_ID = table.Column<int>(type: "int", nullable: true),
                    WorkspaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReportName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project_PBI_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Project_PBI_Reports_Project_PBI_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Project_PBI_Workspaces",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Project_PBI_Reports_Projects_Project_ID",
                        column: x => x.Project_ID,
                        principalTable: "Projects",
                        principalColumn: "Project_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_DataSets_Project_ID",
                table: "Project_PBI_DataSets",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_Reports_Project_ID",
                table: "Project_PBI_Reports",
                column: "Project_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_Reports_WorkspaceId",
                table: "Project_PBI_Reports",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_Workspaces_Project_ID",
                table: "Project_PBI_Workspaces",
                column: "Project_ID");
        }
    }
}

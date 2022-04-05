using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    public partial class UpdatedPowerBiEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PowerBi_Workspaces",
                columns: table => new
                {
                    Workspace_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Workspace_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sandbox_Flag = table.Column<bool>(type: "bit", nullable: false),
                    Project_Id = table.Column<int>(type: "int", nullable: true)
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
                name: "PowerBi_DataSets",
                columns: table => new
                {
                    DataSet_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataSet_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Workspace_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    Report_ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Report_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Workspace_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_DataSets_Workspace_Id",
                table: "PowerBi_DataSets",
                column: "Workspace_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_Reports_Workspace_Id",
                table: "PowerBi_Reports",
                column: "Workspace_Id");

            migrationBuilder.CreateIndex(
                name: "IX_PowerBi_Workspaces_Project_Id",
                table: "PowerBi_Workspaces",
                column: "Project_Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerBi_DataSets");

            migrationBuilder.DropTable(
                name: "PowerBi_Reports");

            migrationBuilder.DropTable(
                name: "PowerBi_Workspaces");
        }
    }
}

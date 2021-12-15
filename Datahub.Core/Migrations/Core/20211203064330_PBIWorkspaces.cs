using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Datahub.Core.Core
{
    public partial class PBIWorkspaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Workspace",
                table: "Project_PBI_Reports");

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceName",
                table: "Project_PBI_Workspaces",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WorkspaceId",
                table: "Project_PBI_Reports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_PBI_Reports_WorkspaceId",
                table: "Project_PBI_Reports",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_PBI_Reports_Project_PBI_Workspaces_WorkspaceId",
                table: "Project_PBI_Reports",
                column: "WorkspaceId",
                principalTable: "Project_PBI_Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_PBI_Reports_Project_PBI_Workspaces_WorkspaceId",
                table: "Project_PBI_Reports");

            migrationBuilder.DropIndex(
                name: "IX_Project_PBI_Reports_WorkspaceId",
                table: "Project_PBI_Reports");

            migrationBuilder.DropColumn(
                name: "WorkspaceName",
                table: "Project_PBI_Workspaces");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Project_PBI_Reports");

            migrationBuilder.AddColumn<Guid>(
                name: "Workspace",
                table: "Project_PBI_Reports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}

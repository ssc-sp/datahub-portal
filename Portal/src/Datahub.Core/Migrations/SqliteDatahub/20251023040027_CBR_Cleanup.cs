using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.SqliteDatahub
{
    /// <inheritdoc />
    public partial class CBR_Cleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.AddColumn<int>(
                name: "Datahub_ProjectProject_ID",
                table: "GCHostingWorkspaceDetails",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GCHostingWorkspaceDetails_Datahub_ProjectProject_ID",
                table: "GCHostingWorkspaceDetails",
                column: "Datahub_ProjectProject_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Datahub_ProjectProject_ID",
                table: "GCHostingWorkspaceDetails",
                column: "Datahub_ProjectProject_ID",
                principalTable: "Projects",
                principalColumn: "Project_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Datahub_ProjectProject_ID",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.DropIndex(
                name: "IX_GCHostingWorkspaceDetails_Datahub_ProjectProject_ID",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.DropColumn(
                name: "Datahub_ProjectProject_ID",
                table: "GCHostingWorkspaceDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                table: "GCHostingWorkspaceDetails",
                column: "Id",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }
    }
}

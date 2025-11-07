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

            migrationBuilder.AddColumn<bool>(
                name: "AnnouncementCreated",
                table: "VersionTags",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnouncementCreated",
                table: "VersionTags");

            migrationBuilder.AddForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                table: "GCHostingWorkspaceDetails",
                column: "Id",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }
    }
}

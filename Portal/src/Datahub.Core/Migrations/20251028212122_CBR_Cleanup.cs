using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
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
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "GCHostingWorkspaceDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnouncementCreated",
                table: "VersionTags");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "GCHostingWorkspaceDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddForeignKey(
                name: "FK_GCHostingWorkspaceDetails_Projects_Id",
                table: "GCHostingWorkspaceDetails",
                column: "Id",
                principalTable: "Projects",
                principalColumn: "Project_ID");
        }
    }
}

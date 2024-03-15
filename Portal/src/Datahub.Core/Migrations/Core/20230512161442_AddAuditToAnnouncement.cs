using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddAuditToAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Announcements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Announcements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedById",
                table: "Announcements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedById",
                table: "Announcements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_UpdatedById",
                table: "Announcements",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_PortalUsers_CreatedById",
                table: "Announcements",
                column: "CreatedById",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_PortalUsers_UpdatedById",
                table: "Announcements",
                column: "UpdatedById",
                principalTable: "PortalUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_PortalUsers_CreatedById",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_PortalUsers_UpdatedById",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_CreatedById",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_UpdatedById",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Announcements");
        }
    }
}

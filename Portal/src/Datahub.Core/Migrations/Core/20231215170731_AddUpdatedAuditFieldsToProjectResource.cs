using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
	/// <inheritdoc />
	public partial class AddUpdatedAuditFieldsToProjectResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequestedById",
                table: "Project_Resources2",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Project_Resources2",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedById",
                table: "Project_Resources2",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources2_RequestedById",
                table: "Project_Resources2",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Resources2_UpdatedById",
                table: "Project_Resources2",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Resources2_PortalUsers_RequestedById",
                table: "Project_Resources2",
                column: "RequestedById",
                principalTable: "PortalUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Resources2_PortalUsers_UpdatedById",
                table: "Project_Resources2",
                column: "UpdatedById",
                principalTable: "PortalUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Resources2_PortalUsers_RequestedById",
                table: "Project_Resources2");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Resources2_PortalUsers_UpdatedById",
                table: "Project_Resources2");

            migrationBuilder.DropIndex(
                name: "IX_Project_Resources2_RequestedById",
                table: "Project_Resources2");

            migrationBuilder.DropIndex(
                name: "IX_Project_Resources2_UpdatedById",
                table: "Project_Resources2");

            migrationBuilder.DropColumn(
                name: "RequestedById",
                table: "Project_Resources2");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Project_Resources2");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Project_Resources2");
        }
    }
}

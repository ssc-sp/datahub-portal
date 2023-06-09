using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddedProjectRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "IsDataApprover",
                table: "Project_Users");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedPortalUserId",
                table: "Project_Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PortalUserId",
                table: "Project_Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Project_Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_ApprovedPortalUserId",
                table: "Project_Users",
                column: "ApprovedPortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_PortalUserId",
                table: "Project_Users",
                column: "PortalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Users_RoleId",
                table: "Project_Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users",
                column: "ApprovedPortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users",
                column: "PortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users",
                column: "RoleId",
                principalTable: "Project_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_ApprovedPortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users");

            migrationBuilder.DropIndex(
                name: "IX_Project_Users_ApprovedPortalUserId",
                table: "Project_Users");

            migrationBuilder.DropIndex(
                name: "IX_Project_Users_PortalUserId",
                table: "Project_Users");

            migrationBuilder.DropIndex(
                name: "IX_Project_Users_RoleId",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "ApprovedPortalUserId",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "PortalUserId",
                table: "Project_Users");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "Project_Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Project_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDataApprover",
                table: "Project_Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

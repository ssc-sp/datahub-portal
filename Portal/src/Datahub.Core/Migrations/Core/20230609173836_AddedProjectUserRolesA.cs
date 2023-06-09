using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations.Core
{
    /// <inheritdoc />
    public partial class AddedProjectUserRolesA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Project_Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedPortalUserId",
                table: "Project_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PortalUserId",
                table: "Project_Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "Project_Users",
                type: "int",
                nullable: true);

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
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_PortalUsers_PortalUserId",
                table: "Project_Users",
                column: "PortalUserId",
                principalTable: "PortalUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Users_Project_Roles_RoleId",
                table: "Project_Users",
                column: "RoleId",
                principalTable: "Project_Roles",
                principalColumn: "Id");
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

            migrationBuilder.AlterColumn<int>(
                name: "Project_ID",
                table: "Project_Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

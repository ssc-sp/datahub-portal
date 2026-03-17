using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datahub.Core.Migrations
{
    /// <inheritdoc />
    public partial class RoleInInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvitedById",
                table: "WorkspaceInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Requested_RoleId",
                table: "WorkspaceInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_InvitedById",
                table: "WorkspaceInvitations",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_Requested_RoleId",
                table: "WorkspaceInvitations",
                column: "Requested_RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvitations_PortalUsers_InvitedById",
                table: "WorkspaceInvitations",
                column: "InvitedById",
                principalTable: "PortalUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkspaceInvitations_Project_Roles_Requested_RoleId",
                table: "WorkspaceInvitations",
                column: "Requested_RoleId",
                principalTable: "Project_Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvitations_PortalUsers_InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkspaceInvitations_Project_Roles_Requested_RoleId",
                table: "WorkspaceInvitations");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceInvitations_InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropIndex(
                name: "IX_WorkspaceInvitations_Requested_RoleId",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "Requested_RoleId",
                table: "WorkspaceInvitations");
        }
    }
}

using System;
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
            migrationBuilder.DropIndex(
                name: "IX_ExternalUsers_ExternalSubject",
                table: "ExternalUsers");

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

            migrationBuilder.AlterColumn<string>(
                name: "ExternalSubject",
                table: "ExternalUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_InvitedById",
                table: "WorkspaceInvitations",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvitations_Requested_RoleId",
                table: "WorkspaceInvitations",
                column: "Requested_RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_ExternalSubject",
                table: "ExternalUsers",
                column: "ExternalSubject",
                unique: true,
                filter: "[ExternalSubject] IS NOT NULL");

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

            migrationBuilder.DropIndex(
                name: "IX_ExternalUsers_ExternalSubject",
                table: "ExternalUsers");

            migrationBuilder.DropColumn(
                name: "InvitedById",
                table: "WorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "Requested_RoleId",
                table: "WorkspaceInvitations");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalSubject",
                table: "ExternalUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalUsers_ExternalSubject",
                table: "ExternalUsers",
                column: "ExternalSubject",
                unique: true);
        }
    }
}
